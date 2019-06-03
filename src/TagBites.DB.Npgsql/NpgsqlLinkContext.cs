using Npgsql;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace TBS.Data.DB.PostgreSql
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

        internal NpgsqlLinkContext(DbLinkProvider linkProvider, Action<DbConnectionStringBuilder> connectionStringAdapter)
            : base(linkProvider, connectionStringAdapter)
        {
            ConnectionOpen += OnConnectionOpen;
        }


        private void OnConnectionOpen(object sender, EventArgs e)
        {
            var connection = (NpgsqlConnection)GetConnection();

            if (connection != null)
            {
                connection.Notification += OnNotification;
                connection.Notice += OnNotice;
                //connection.Commiting += OnCommiting;

                //try
                //{
                //    connection.TypeMapper.AddMapping(new NpgsqlTypeMappingBuilder
                //    {
                //        PgTypeName = "mpq",
                //        ClrTypes = new[] { typeof(Fraction) },
                //        TypeHandlerFactory = new FractionHandlerFactory()
                //    }.Build());
                //}
                //catch
                //{

                //}
            }
        }

        private void OnNotification(object sender, NpgsqlNotificationEventArgs e)
        {
            OnNotify(e.PID, e.Condition, e.AdditionalInformation);
        }
        private void OnNotice(object sender, NpgsqlNoticeEventArgs e)
        {
            OnInfoMessage(e.Notice.MessageText, e.Notice.Where);
        }
        private void OnCommiting(object sender, EventArgs e)
        {
            OnTransactionBeforeCommit();
        }

        protected override Task WaitAsync(CancellationToken token)
        {
            var connection = (NpgsqlConnection)GetOpenConnection();
            return connection.WaitAsync(token);
        }
        protected override bool IsConnectionBrokenException(Exception ex)
        {
            return base.IsConnectionBrokenException(ex)
                || ex.Message == "The Connection is broken.";
        }
    }
}
