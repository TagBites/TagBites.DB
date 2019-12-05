using System;
using System.Threading;
using System.Threading.Tasks;

namespace TagBites.DB.Postgres
{
    public class PgSqlNotificationEventArgs : EventArgs
    {
        public int ProcessId { get; }
        public string Channel { get; }
        public string Message { get; }

        public PgSqlNotificationEventArgs(int pid, string channel, string message)
        {
            ProcessId = pid;
            Channel = channel;
            Message = message;
        }
    }

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

        public abstract int? ProcessId { get; }


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

        protected void OnNotify(int pid, string channel, string message)
        {
            _notification?.Invoke(this, new PgSqlNotificationEventArgs(pid, channel, message));
        }
    }
}
