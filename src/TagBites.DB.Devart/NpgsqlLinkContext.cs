using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Devart.Data.PostgreSql;
using TagBites.DB.Postgres;

namespace TagBites.DB.Npgsql
{
    public class NpgsqlLinkContext : PgSqlLinkContext
    {
        private int? _processId;

        public override int? ProcessId
        {
            get
            {
                lock (SynchRoot)
                {
                    if (GetConnection() != null)
                        return _processId ??= this.ExecuteScalar<int>("select pg_backend_pid()");

                    return null;
                }
            }
        }

        protected internal NpgsqlLinkContext()
        { }


        protected override void OnConnectionCreated()
        {
            base.OnConnectionCreated();

            _processId = null;

            var connection = (PgSqlConnection)GetConnection();
            connection.Notification += OnNotification;
            connection.InfoMessage += OnInfoMessage;
        }


        private void OnNotification(object sender, Devart.Data.PostgreSql.PgSqlNotificationEventArgs e)
        {
            OnNotify(e.ProcessId, e.Condition, e.AdditionalMessage);
        }
        private void OnInfoMessage(object sender, PgSqlInfoMessageEventArgs e)
        {
            OnInfoMessage(e.Message, e.Errors.Cast<PgSqlError>().FirstOrDefault()?.CallStack);
        }

        protected override async Task StartNotifyListenerTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                this.ExecuteNonQuery("Select 1");
                await Task.Delay(1000, token).ConfigureAwait(false);
            }
        }
        protected override bool IsConnectionBrokenException(Exception ex)
        {
            return base.IsConnectionBrokenException(ex)
                || ex.Message == "The Connection is broken.";
        }
    }
}
