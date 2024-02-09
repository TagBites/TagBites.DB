using System;

namespace TagBites.DB.Postgres;

public class PgSqlNotificationEventArgs : EventArgs
{
    public PgSqlNotification Notification { get; }

    public int ProcessId => Notification.ProcessId;
    public string Channel => Notification.Channel;
    public string Message => Notification.Message;

    public PgSqlNotificationEventArgs(PgSqlNotification notification) => Notification = notification;
    public PgSqlNotificationEventArgs(int processId, string channel, string message)
    {
        Notification = new PgSqlNotification(processId, channel, message);
    }
}
