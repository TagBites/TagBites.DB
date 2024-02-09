using System.ComponentModel;

namespace TagBites.DB.Postgres;

[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void PgSqlNotificationInternalDelegate(in PgSqlNotification notification);
