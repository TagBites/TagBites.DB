using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql.PostgreSql
{
    public class PqSqlQueryResolver : SqlQueryResolver
    {
        protected internal override void VisitQuery(SqlQuerySelect query, SqlQueryBuilder builder)
        {
            base.VisitQuery(query, builder);

            var pgSqlQuery = query as PqSqlQuerySelect;
            if (pgSqlQuery != null)
                VisitClause(pgSqlQuery.Locking, builder);
        }

        protected internal virtual void VisitClause(PgSqlClauseLocking clause, SqlQueryBuilder builder)
        {
            for (int i = 0; i < clause.Count; i++)
                VisitClauseEntry(clause[i], builder);
        }

        protected internal virtual void VisitClauseEntry(PgSqlClauseLockingEntry entry, SqlQueryBuilder builder)
        {
            switch (entry.LockingType)
            {
                case PgSqlClauseLockingEntryType.Update: builder.AppendKeyword("FOR UPDATE"); break;
                case PgSqlClauseLockingEntryType.NoKeyUpdate: builder.AppendKeyword("FOR NO KEY UPDATE"); break;
                case PgSqlClauseLockingEntryType.Share: builder.AppendKeyword("FOR SHARE"); break;
                case PgSqlClauseLockingEntryType.KeyShare: builder.AppendKeyword("FOR KEY SHARE"); break;
                default: throw new ArgumentOutOfRangeException($"LockingType = {entry.LockingType} is not supported.");
            }

            if (!string.IsNullOrEmpty(entry.TableName))
            {
                builder.AppendKeyword("OF");
                builder.Append(QuoteTableNameIfNeeded(entry.TableName));
            }

            if (entry.WaitMode != PgSqlClauseLockingEntryWaitMode.Default)
            {
                switch (entry.WaitMode)
                {
                    case PgSqlClauseLockingEntryWaitMode.Nowait: builder.AppendKeyword("NOWAIT"); break;
                    case PgSqlClauseLockingEntryWaitMode.SkipLocked: builder.AppendKeyword("SKIP LOCKED"); break;
                    default: throw new ArgumentOutOfRangeException($"WaitMode = {entry.WaitMode} is not supported.");
                }
            }
        }
        protected override void VisitTableClause(SqlTable table, string keyword, SqlQueryBuilder builder)
        {
            if (keyword != null)
                builder.AppendKeyword(keyword);

            VisitTableDeclaration(builder, table, false, keyword != "INSERT INTO");
        }


        protected override string GetCastString(object value, string typeName)
        {
            return value is string ? $"'{value}'::{typeName}" : $"({value})::{typeName}";
        }

        protected override string GetBuildInFunctionName(string functionName)
        {
            if (String.Equals(functionName, nameof(SqlFunction.TrimStart), StringComparison.OrdinalIgnoreCase))
                return "LTRIM";
            else if (String.Equals(functionName, nameof(SqlFunction.TrimEnd), StringComparison.OrdinalIgnoreCase))
                return "RTRIM";

            return base.GetBuildInFunctionName(functionName);
        }
    }
}
