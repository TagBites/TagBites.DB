using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql.PostgreSql
{
    public class PgSqlClauseLocking : SqlClauseCollectionBase<PgSqlClauseLockingEntry>
    {
        public void Add(PgSqlClauseLockingEntryType lockingType)
        {
            Add(lockingType, null, PgSqlClauseLockingEntryWaitMode.Default);
        }
        public void Add(PgSqlClauseLockingEntryType lockingType, string tableName)
        {
            Add(lockingType, tableName, PgSqlClauseLockingEntryWaitMode.Default);
        }
        public void Add(PgSqlClauseLockingEntryType lockingType, PgSqlClauseLockingEntryWaitMode waitMode)
        {
            Add(lockingType, null, waitMode);
        }
        public void Add(PgSqlClauseLockingEntryType lockingType, string tableName, PgSqlClauseLockingEntryWaitMode waitMode)
        {
            Add(new PgSqlClauseLockingEntry(lockingType, tableName, waitMode));
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            if (resolver is PqSqlQueryResolver)
                ((PqSqlQueryResolver)resolver).VisitClause(this, builder);
        }
    }
}