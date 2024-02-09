using System;
using System.Threading;
using System.Threading.Tasks;

namespace TagBites.DB.Postgres
{
    public abstract class PgSqlLinkContext : DbLinkContext
    {
        private EventHandler<PgSqlNotificationEventArgs> _notification;

        public event EventHandler<PgSqlNotificationEventArgs> Notification
        {
            add
            {
                CheckDispose();
                _notification += value;
            }
            remove
            {
                CheckDispose();
                _notification -= value;
            }
        }
        protected internal PgSqlNotificationInternalDelegate NotificationHandlerInternal;

        public abstract int? ProcessId { get; }

        protected PgSqlLinkContext()
        {
            NotificationHandlerInternal = OnNotify;
        }


        public void Notify(string chanelName, string message)
        {
            if (string.IsNullOrEmpty(chanelName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(chanelName));

            if (string.IsNullOrEmpty(message))
                ExecuteNonQuery(new Query("NOTIFY " + chanelName));
            else
                ExecuteNonQuery(new Query($"NOTIFY {chanelName}, '{message}'"));
        }
        public void Listen(params string[] channelsNames)
        {
            if (channelsNames == null)
                throw new ArgumentNullException(nameof(channelsNames));
            if (channelsNames.Length == 0)
                return;

            ExecuteNonQuery(new Query("LISTEN " + string.Join("; LISTEN ", channelsNames)));
        }
        public void Unlisten(params string[] channelsNames)
        {
            if (channelsNames == null)
                throw new ArgumentNullException(nameof(channelsNames));
            if (channelsNames.Length == 0)
                return;

            ExecuteNonQuery(new Query("UNLISTEN " + string.Join("; UNLISTEN ", channelsNames)));
        }
        public void UnlistenAll()
        {
            ExecuteNonQuery(new Query("UNLISTEN *"));
        }

        protected internal abstract Task StartNotifyListenerTask(CancellationToken token);
        internal IDbLinkTransaction BeginForCursorManager() => Begin(true, false);

        private void OnNotify(in PgSqlNotification notification) => _notification?.Invoke(this, new PgSqlNotificationEventArgs(notification));
    }
}
