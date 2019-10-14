using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql.Postgres.Clauses
{
    public enum PgSqlClauseLockingEntryWaitMode
    {
        Default,
        Nowait,
        SkipLocked
    }
}