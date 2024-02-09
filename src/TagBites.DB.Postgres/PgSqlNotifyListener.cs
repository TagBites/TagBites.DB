#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TagBites.Utils;

namespace TagBites.DB.Postgres;

public class PgSqlNotifyListener : IDisposable
{
    private PgSqlLinkProvider? _linkProvider;
    private PgSqlLink? _link;
    private readonly HashSet<string> _channels = new();

    private readonly SemaphoreSlim _semaphore = new(1);
    private Task? _listenerTask;
    private CancellationTokenSource? _listenerCancellationToken;
    private bool _enabled;

    private TimeSpan _broadcastDelay;
    private readonly ContinueActionConsumer _notificationBroadcaster;
    private readonly List<PgSqlNotification> _pendingNotifications = [];

    public event EventHandler? ConnectionOpened;
    public event EventHandler? ConnectionLost;
    public event EventHandler<PgSqlNotificationEventArgs>? Notification;
    public event EventHandler<PgSqlBatchNotificationEventArgs>? BatchNotification;

    public int? ProcessId => _link?.ConnectionContext.ProcessId;
    public TimeSpan BroadcastDelay
    {
        get => _broadcastDelay;
        set
        {
            if (value.Ticks < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            _broadcastDelay = value;
        }
    }

    public PgSqlNotifyListener(PgSqlLinkProvider linkProvider)
    {
        _linkProvider = linkProvider ?? throw new ArgumentNullException(nameof(linkProvider));
        _notificationBroadcaster = new ContinueActionConsumer(SendNotificationsAsync);
    }
    ~PgSqlNotifyListener()
    {
        Debug.WriteLine($"Unexpected finalizer called on IDisposable object {nameof(PgSqlNotifyListener)}.");
        Dispose(false);
    }


    public Task<bool> ListenAsync(string channel) => ListenAsync(new[] { channel });
    public async Task<bool> ListenAsync(params string[] channels)
    {
        CheckDispose();

        var changes = new List<string>(channels.Length);

        lock (_channels)
            changes.AddRange(channels.Where(_channels.Add));

        if (changes.Count == 0)
            return false;

        await _semaphore.WaitAsync();
        try
        {
            await StopListenerAsync();

            if (_link == null)
            {
                PrepareLink();
                _link!.Force(); // Listen during connection open
            }
            else
                _link!.Listen(changes.ToArray());

            StartListener();
        }
        finally
        {
            _semaphore.Release();
        }

        return true;
    }

    public Task<bool> UnlistenAsync(string channel) => UnlistenAsync(new[] { channel });
    public async Task<bool> UnlistenAsync(params string[] channels)
    {
        CheckDispose();

        var changes = new List<string>(channels.Length);
        bool anyLeft;

        lock (_channels)
        {
            changes.AddRange(channels.Where(_channels.Remove));
            anyLeft = _channels.Count > 0;
        }

        if (changes.Count == 0)
            return false;

        await _semaphore.WaitAsync();
        try
        {
            await StopListenerAsync();
            _link!.Unlisten(changes.ToArray());

            if (anyLeft)
                StartListener();

            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void StartListener()
    {
        if (!_enabled)
        {
            try
            {
                _enabled = true;
                _listenerCancellationToken = new CancellationTokenSource();
                _listenerTask = _link!.ConnectionContext.StartNotifyListenerTask(_listenerCancellationToken.Token);
            }
            catch
            {
                DisposeListener();
                throw;
            }
        }
    }
    private async Task StopListenerAsync()
    {
        if (_enabled)
        {
            try
            {
                _listenerCancellationToken?.Cancel();
                _listenerCancellationToken?.Dispose();

                if (_listenerTask != null)
                    await _listenerTask;
            }
            catch
            {
                // Ignored
            }
            finally
            {
                _listenerCancellationToken = null;
                _listenerTask = null;
                _enabled = false;
            }
        }
    }

    private void PrepareLink()
    {
        if (_link == null)
        {
            _link = _linkProvider!.CreateExclusiveNotifyLink();

            _link.ConnectionContext.ConnectionOpened += OnConnectionOpened;
            _link.ConnectionContext.ConnectionLost += OnConnectionLost;

            _link.ConnectionContext.NotificationHandlerInternal = OnNotification;
        }
    }
    private void OnConnectionOpened(object sender, EventArgs e)
    {
        ConnectionOpened?.Invoke(sender, e);

        string[] channels;

        lock (_channels)
        {
            if (_channels.Count == 0)
                return;

            channels = _channels.ToArray();
        }

        _link!.Listen(channels);
    }
    private void OnConnectionLost(object sender, DbLinkConnectionLostEventArgs e)
    {
        ConnectionLost?.Invoke(sender, e);

        if (!e.Reconnect)
        {
            Thread.Sleep(1000);
            e.Reconnect = true;
        }
    }

    private void OnNotification(in PgSqlNotification notification)
    {
        lock (_pendingNotifications)
            _pendingNotifications.Add(notification);

        try
        {
            _ = _notificationBroadcaster.ProcessAsync();
        }
        catch
        {
            // ignored
        }
    }
    private async Task SendNotificationsAsync()
    {
        // Wait for more notify
        await Task.Delay(BroadcastDelay);

        // Get current batch
        PgSqlNotification[] notifications;

        lock (_pendingNotifications)
        {
            notifications = _pendingNotifications.ToArray();
            _pendingNotifications.Clear();
        }

        // Broadcast
        BatchNotification?.Invoke(this, new PgSqlBatchNotificationEventArgs(notifications));

        if (Notification is { } n)
        {
            foreach (var item in notifications)
                n(this, new PgSqlNotificationEventArgs(item.ProcessId, item.Channel, item.Message));
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private void Dispose(bool _)
    {
        if (_linkProvider == null)
            return;

        DisposeListener();

        if (_link != null)
        {
            try { _link.Dispose(); }
            finally { _link = null; }
        }

        _linkProvider = null;
        _semaphore.Dispose();
    }
    private void DisposeListener()
    {
        try
        {
            _listenerCancellationToken?.Cancel();
            _listenerCancellationToken?.Dispose();
        }
        catch
        {
            // Ignored
        }
        finally
        {
            _listenerCancellationToken = null;
            _listenerTask = null;
            _enabled = false;
        }
    }

    private void CheckDispose()
    {
        if (_linkProvider == null)
            throw new ObjectDisposedException(nameof(PgSqlNotifyListener));
    }
}
