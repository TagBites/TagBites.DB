using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using TagBites.DB.Postgres;

namespace TagBites.DB.Npgsql
{
    public class NpgsqlLinkContext : PgSqlLinkContext
    {
        public override int? ProcessId
        {
            get
            {
                lock (SynchRoot)
                {
                    if (GetConnection() is NpgsqlConnection connection)
                    {
                        if ((connection.State & ConnectionState.Open) != 0)
                            return connection.ProcessID;

                        var status = TransactionStatus;
                        if (status == DbLinkTransactionStatus.None || status == DbLinkTransactionStatus.Open || status == DbLinkTransactionStatus.Pending)
                            return ExecuteInner(() => ((NpgsqlConnection)GetConnection()).ProcessID);
                    }

                    return null;
                }
            }
        }

        protected internal NpgsqlLinkContext()
        { }


        protected override void OnConnectionCreated()
        {
            base.OnConnectionCreated();

            var connection = (NpgsqlConnection)GetConnection();
            connection.Notification += OnNotification;
            connection.Notice += OnNotice;
            //connection.Commiting += OnCommiting;
        }

        private void OnNotification(object sender, NpgsqlNotificationEventArgs e)
        {
            OnNotify(e.PID, e.Channel, e.Payload);
        }
        private void OnNotice(object sender, NpgsqlNoticeEventArgs e)
        {
            OnInfoMessage(e.Notice.MessageText, e.Notice.Where);
        }
        private void OnCommiting(object sender, EventArgs e)
        {
            OnTransactionBeforeCommit();
        }

        protected override Task StartNotifyListenerTask(CancellationToken token)
        {
            return Task.Run(async () =>
                 {
                     while (!token.IsCancellationRequested)
                         await ((NpgsqlConnection)GetOpenConnection()).WaitAsync(token).ConfigureAwait(false);
                 },
                 token);
        }
        protected override bool IsConnectionBrokenException(Exception ex)
        {
            return base.IsConnectionBrokenException(ex)
                || ex.Message == "The Connection is broken.";
        }
    }
}
