using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using TagBites.DB.Postgres;

namespace TagBites.DB.Npgsql
{
    public class NpgsqlLinkContext : PgSqlLinkContext
    {
        private int? _processId;

        public override int? ProcessId => _processId;

        protected internal NpgsqlLinkContext()
        { }


        protected override void OnConnectionCreated()
        {
            base.OnConnectionCreated();

            var connection = (NpgsqlConnection)GetConnection();
            connection.Notification += OnNotification;
            connection.Notice += OnNotice;
        }
        protected override void OnConnectionOpen()
        {
            base.OnConnectionOpen();
            _processId = ((NpgsqlConnection)GetConnection()).ProcessID;
        }
        protected override void OnConnectionDisposed()
        {
            base.OnConnectionDisposed();
            _processId = null;
        }

        private void OnNotification(object sender, NpgsqlNotificationEventArgs e)
        {
            NotificationHandlerInternal(new PgSqlNotification(e.PID, e.Channel, e.Payload));
        }
        private void OnNotice(object sender, NpgsqlNoticeEventArgs e)
        {
            OnInfoMessage(e.Notice.MessageText, e.Notice.Where);
        }

        protected override Task StartNotifyListenerTask(CancellationToken token)
        {
            return Task.Run(ListenAsync, token);

            async Task ListenAsync()
            {
                while (!token.IsCancellationRequested)
                {
                    var connection = (NpgsqlConnection)GetOpenConnection();
                    try
                    {
                        try
                        {
                            while (!token.IsCancellationRequested)
                                await connection.WaitAsync(token);
                        }
                        catch when (token.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                    catch
                    {
                        await Task.Delay(100, token);
                    }
                }
            }
        }
        protected override bool IsConnectionBrokenException(Exception ex)
        {
            return base.IsConnectionBrokenException(ex)
                || ex.Message == "The Connection is broken."
                || ex is NpgsqlException ne && ne.Message?.StartsWith("Unknown message code:") == true;
        }
    }
}
