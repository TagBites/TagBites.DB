﻿namespace TagBites.Sql.Postgres
{
    public class PgSqlClauseLockingEntry : SqlClauseEntry
    {
        public PgSqlClauseLockingEntryType LockingType { get; }
        public PgSqlClauseLockingEntryWaitMode WaitMode { get; }
        public string TableName { get; }

        public PgSqlClauseLockingEntry(PgSqlClauseLockingEntryType lockingType, string tableName, PgSqlClauseLockingEntryWaitMode waitMode)
        {
            LockingType = lockingType;
            TableName = tableName;
            WaitMode = waitMode;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            if (resolver is PqSqlQueryResolver)
                ((PqSqlQueryResolver)resolver).VisitClauseEntry(this, builder);
            else
                builder.AppendOrThrowNotSupportedByResolver(this, resolver);
        }
    }
}
