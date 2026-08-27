using TagBites.Utils;

namespace TagBites.Sql.Sqlite
{
    public class SqliteQueryResolver : SqlQueryResolver
    {
        protected override string TrueLiteral => "1";
        protected override string FalseLiteral => "0";


        protected internal override void VisitQuery(SqlQuerySelect query, SqlQueryBuilder builder)
        {
            MoveToWithIfNeeded(query.From, query.With);

            base.VisitQuery(query, builder);
        }
        protected internal override void VisitQuery(SqlQueryUpdate query, SqlQueryBuilder builder)
        {
            MoveToWithIfNeeded(query.From, query.With);

            base.VisitQuery(query, builder);
        }
        protected internal override void VisitQuery(SqlQueryDelete query, SqlQueryBuilder builder)
        {
            MoveToWithIfNeeded(query.Using, query.With);
            query.From.Alias = null;

            base.VisitQuery(query, builder);
        }
        protected internal override void VisitQuery(SqlQueryInsertBase query, SqlQueryBuilder builder)
        {
            query.Into.Alias = null;

            base.VisitQuery(query, builder);
        }
        protected override void VisitReturningClause(SqlClauseSelect clause, SqlQueryBuilder builder)
        {
            // Table qualifier on a RETURNING column is not supported
            var columns = new SqlClauseSelect();

            foreach (var entry in clause)
                if (entry.Expression is SqlColumn column)
                    columns.Add(SqlExpression.Literal(QuoteIdentifierIfNeeded(column.ColumnName)), entry.Alias);
                else
                    columns.Add(entry.Expression, entry.Alias);

            base.VisitReturningClause(columns, builder);
        }
        protected override void VisitLimitOffset(int? limit, int? offset, SqlQueryBuilder builder)
        {
            // OFFSET must follow a LIMIT (-1 means no limit)
            if (!limit.HasValue && offset.HasValue)
                limit = -1;

            base.VisitLimitOffset(limit, offset, builder);
        }
        protected override void VisitUnionBranch(object query, SqlQueryBuilder builder)
        {
            // No parentheses between UNION keywords
            if (query is SqlQueryBase queryBase)
                queryBase.Accept(this, builder);
            else
                base.VisitUnionBranch(query, builder);
        }

        private void MoveToWithIfNeeded(SqlClauseFrom fromClause, SqlClauseWith withClause)
        {
            for (int i = fromClause.Count - 1; i >= 0; i--)
                if (fromClause[i].ColumnNames.Length > 0)
                {
                    var table = fromClause[i].Table.Table;
                    SqlClauseWithEntry withEntry;
                    if (table is string)
                    {
                        var q = new SqlQuerySelect();
                        q.Select.AddAll();
                        q.From.Add((string)table);
                        withEntry = new SqlClauseWithEntry(fromClause[i].Table.Alias, fromClause[i].ColumnNames, q);
                    }
                    else if (table is SqlLiteralExpression)
                        withEntry = new SqlClauseWithEntry(fromClause[i].Table.Alias, fromClause[i].ColumnNames, (SqlLiteralExpression)table);
                    else if (table is SqlLiteral)
                        withEntry = new SqlClauseWithEntry(fromClause[i].Table.Alias, fromClause[i].ColumnNames, (SqlLiteral)table);
                    else if (table is SqlQueryBase)
                        withEntry = new SqlClauseWithEntry(fromClause[i].Table.Alias, fromClause[i].ColumnNames, (SqlQueryBase)table);
                    else
                        throw new NotSupportedException();

                    fromClause.RemoveAt(i);
                    withClause.Add(withEntry);

                    fromClause.Add(withEntry, withEntry.Name);
                }
        }

        protected override void VisitTableClause(SqlTable table, string keyword, SqlQueryBuilder builder)
        {
            if (keyword != null)
                builder.AppendKeyword(keyword);

            // Alias is not supported for INSERT INTO and DELETE FROM
            VisitTableDeclaration(builder, table, false, keyword != "INSERT INTO" && keyword != "DELETE FROM");
        }

        protected override string GetOperatorTypeString(SqlConditionBinaryOperatorType operatorType)
        {
            if (operatorType == SqlConditionBinaryOperatorType.ILike)
                operatorType = SqlConditionBinaryOperatorType.Like;

            return base.GetOperatorTypeString(operatorType);
        }

        protected override string ToDateTimeParameterString(DateTime value)
        {
            return $"'{value:o}'";
        }
        protected override string ToTimeSpanParameterString(TimeSpan value)
        {
            return $"'{value:o}'";
        }

        public override string GetTypeName(Type netType)
        {
            var nullableType = Nullable.GetUnderlyingType(netType);
            if (nullableType != null)
                netType = nullableType;

            switch (DataHelper.GetTypeCode(netType))
            {
                case TypeCode.Empty:
                case (TypeCode)2:
                    return "NULL";

                case TypeCode.Boolean:
                    return "BOOL";

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return "INT";

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return "BIGINT";

                case TypeCode.Single:
                    return "FLOAT";
                case TypeCode.Double:
                    return "REAL";
                case TypeCode.Decimal:
                    return "NUMERIC";

                //case TypeCode.DateTime:
                //    return "DATETIME";

                default:
                    return "TEXT";
            }
        }
        protected override string GetBuildInFunctionName(string functionName)
        {
            if (functionName == nameof(SqlFunction.Substring))
                return "SUBSTR";
            else if (string.Equals(functionName, nameof(SqlFunction.TrimStart), StringComparison.OrdinalIgnoreCase))
                return "LTRIM";
            else if (string.Equals(functionName, nameof(SqlFunction.TrimEnd), StringComparison.OrdinalIgnoreCase))
                return "RTRIM";

            return base.GetBuildInFunctionName(functionName);
        }
    }
}
