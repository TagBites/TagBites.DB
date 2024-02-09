using System;
using System.Collections.Generic;

namespace TagBites.DB.Postgres;

public class PgSqlBatchNotificationEventArgs : EventArgs
{
    public IList<PgSqlNotification> Notifications { get; }

    public PgSqlBatchNotificationEventArgs(IList<PgSqlNotification> notifications) => Notifications = notifications;
}
