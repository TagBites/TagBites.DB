using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TagBites.DB.Postgres
{
    public class PgSqlNotifyListener : IDisposable
    {
        private PgSqlLinkProvider _linkProvider;
        private PgSqlLink _link;
        private readonly HashSet<string> _channels = new();

        private readonly SemaphoreSlim _listenerSemaphore = new(1);
        private Task _listenerTask;
        private CancellationTokenSource _listenerCancellationToken;
        private bool _enabled;

        public event EventHandler<PgSqlNotificationEventArgs> Notification;

        public int? ProcessId => _link?.ConnectionContext.ProcessId;

        public PgSqlNotifyListener(PgSqlLinkProvider linkProvider)
        {
            _linkProvider = linkProvider ?? throw new ArgumentNullException(nameof(linkProvider));
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

            await _listenerSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var changes = channels.Where(x => !_channels.Contains(x)).ToArray();
                if (changes.Length == 0)
                    return false;

                await StopListenerAsync().ConfigureAwait(false);
                {
                    PrepareLink();
                    _link.Listen(changes);
                }
                StartListener();

                foreach (var change in changes)
                    _channels.Add(change);

                return true;
            }
            finally
            {
                _listenerSemaphore.Release();
            }
        }

        public Task<bool> UnlistenAsync(string channel) => UnlistenAsync(new[] { channel });
        public async Task<bool> UnlistenAsync(params string[] channels)
        {
            CheckDispose();

            await _listenerSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var changes = channels.Where(_channels.Remove).ToArray();
                if (changes.Length == 0)
                    return false;

                await StopListenerAsync().ConfigureAwait(false);
                {
                    _link.Unlisten(changes);
                }
                if (_channels.Count > 0)
                    StartListener();

                return true;
            }
            finally
            {
                _listenerSemaphore.Release();
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
                    _listenerTask = _link.ConnectionContext.StartNotifyListenerTask(_listenerCancellationToken.Token);
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
                        await _listenerTask.ConfigureAwait(false);
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
                _link = _linkProvider.CreateExclusiveNotifyLink();
                _link.ConnectionContext.ConnectionOpened += LinkConnectionOpened;
                _link.ConnectionContext.ConnectionLost += Link_ConnectionLost;
                _link.ConnectionContext.Notification += Link_Notification;
            }
        }
        private void LinkConnectionOpened(object sender, EventArgs e)
        {
            if (_channels.Count > 0)
                _link.Listen(_channels.ToArray());
        }
        private void Link_ConnectionLost(object sender, DbLinkConnectionLostEventArgs e)
        {
            if (!e.Reconnect)
            {
                Thread.Sleep(1000);
                e.Reconnect = true;
            }
        }
        private void Link_Notification(object sender, PgSqlNotificationEventArgs e)
        {
            if (_enabled)
                Notification?.Invoke(this, e);
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
            _listenerSemaphore.Dispose();
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
}
