using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql.PostgreSql
{
    public enum PgSqlClauseLockingEntryType
    {
        Update,
        NoKeyUpdate,
        Share,
        KeyShare
    }
}