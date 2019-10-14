using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TagBites.DB.Postgres
{
    public class PgSqlNotifyListener : IDisposable
    {
        private PgSqlLinkProvider _linkProvider;
        private PgSqlLink _link;
        private readonly HashSet<string> _channels = new HashSet<string>();

        private readonly SemaphoreSlim _listenerSemaphore = new SemaphoreSlim(1);
        private Task _listenerTask;
        private CancellationTokenSource _listenerCancellationToken;
        private bool _enabled;

        public event EventHandler<PgSqlNotificationEventArgs> Notification;

        public PgSqlNotifyListener(PgSqlLinkProvider linkProvider)
        {
            _linkProvider = linkProvider ?? throw new ArgumentNullException(nameof(linkProvider));
        }
        ~PgSqlNotifyListener()
        {
            Debug.WriteLine($"Unexpected finalizer called on IDisposable object {nameof(PgSqlNotifyListener)}.");
            Dispose(false);
        }


        public async Task<bool> ListenAsync(string channel)
        {
            lock (_channels)
                if (!_channels.Add(channel))
                    return false;

            PrepareLink();
            await ListenCore(new[] { channel }).ConfigureAwait(false);

            return true;
        }
        public async Task<bool> ListenAsync(params string[] channels)
        {
            string[] changes;

            lock (_channels)
                changes = channels.Where(_channels.Add).ToArray();

            if (changes.Length == 0)
                return false;

            PrepareLink();
            await ListenCore(changes.ToArray()).ConfigureAwait(false);

            return true;
        }
        public async Task<bool> UnlistenAsync(string channel)
        {
            lock (_channels)
                if (!_channels.Remove(channel))
                    return false;

            await UnlistenCore(new[] { channel }).ConfigureAwait(false);
            return true;
        }
        public async Task<bool> UnlistenAsync(params string[] channels)
        {
            string[] changes;

            lock (_channels)
                changes = channels.Where(_channels.Remove).ToArray();

            if (changes.Length == 0)
                return false;

            await UnlistenCore(changes).ConfigureAwait(false);
            return true;
        }

        private async Task ListenCore(string[] channels)
        {
            await _listenerSemaphore.WaitAsync().ConfigureAwait(false);
            {
                await StopListener().ConfigureAwait(false);

                _link.Listen(channels);

                StartListener();
            }
            _listenerSemaphore.Release();
        }
        private async Task UnlistenCore(string[] channels)
        {
            await _listenerSemaphore.WaitAsync().ConfigureAwait(false);
            {
                await StopListener().ConfigureAwait(false);

                _link.Unlisten(channels);

                lock (_channels)
                    if (_channels.Count > 0)
                        StartListener();
            }
            _listenerSemaphore.Release();
        }
        private void StartListener()
        {
            if (!_enabled)
            {
                PrepareLink();

                _listenerCancellationToken = new CancellationTokenSource();
                var token = _listenerCancellationToken.Token;

                _listenerTask = Task.Run(async () =>
                    {
                        while (!token.IsCancellationRequested)
                            await _link.ConnectionContext.WaitAsync(token);
                    },
                    token);

                _enabled = true;
            }
        }
        private async Task StopListener()
        {
            if (_enabled)
            {
                try
                {
                    _listenerCancellationToken.Cancel();
                    await _listenerTask.ConfigureAwait(false);
                }
                catch
                {
                    // Ignored
                }
                finally
                {
                    _listenerCancellationToken.Dispose();

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
                CheckDispose();

                _link = _linkProvider.CreateExclusiveNotifyLink();
                _link.ConnectionContext.ConnectionOpen += Link_ConnectionOpen;
                _link.ConnectionContext.ConnectionLost += Link_ConnectionLost;
                _link.ConnectionContext.Notification += Link_Notification;
            }
        }
        private void Link_ConnectionOpen(object sender, EventArgs e)
        {
            string[] channels;

            lock (_channels)
                channels = _channels.ToArray();

            foreach (var chanelName in channels)
                _link.Listen(chanelName);
        }
        private void Link_ConnectionLost(object sender, DbLinkConnectionLostEventArgs e)
        {
            try
            {
                _listenerCancellationToken.Dispose();
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

            if (!e.Reconnect)
            {
                Thread.Sleep(1000);
                e.Reconnect = true;
            }
        }
        private void Link_Notification(object sender, PgSqlNotificationEventArgs e)
        {
            var eh = Notification;
            if (eh == null)
                return;

            Task.Run(() => eh.Invoke(this, e));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private async void Dispose(bool disposing)
        {
            if (_linkProvider == null)
                return;

            _linkProvider = null;

            if (_link != null)
            {
                await StopListener().ConfigureAwait(false);

                try { _link.Dispose(); }
                finally { _link = null; }
            }

            _listenerSemaphore.Dispose();
        }

        private void CheckDispose()
        {
            if (_linkProvider == null)
                throw new ObjectDisposedException(nameof(PgSqlNotifyListener));
        }
    }
}
