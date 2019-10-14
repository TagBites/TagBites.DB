using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql.Postgres.Clauses
{
    public enum PgSqlClauseLockingEntryType
    {
        Update,
        NoKeyUpdate,
        Share,
        KeyShare
    }
}