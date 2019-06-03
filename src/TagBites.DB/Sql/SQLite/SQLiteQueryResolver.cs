using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TBS.Sql;
using TBS.Utils;

namespace TBS.Sql.SQLite
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteQueryResolver : SqlQueryResolver
    {
        public override bool SupportReturningClause => false;

        protected override string TrueLiteral { get; } = "1";
        protected override string FalseLiteral { get; } = "0";


        protected internal override void VisitQuery(SqlQuerySelect query, SqlQueryBuilder builder)
        {
            MoveToWithIfNeeded(query.From, query.With);

            base.VisitQuery(query, builder);
        }
        protected internal override void VisitQuery(SqlQueryUpdate query, SqlQueryBuilder builder)
        {
            MoveToWithIfNeeded(query.From, query.With);
            query.Table.Alias = null;

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

            VisitTableDeclaration(builder, table, false, keyword != "INSERT INTO" && keyword != "DELETE FROM" && keyword != "UPDATE");
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
            else if (String.Equals(functionName, nameof(SqlFunction.TrimStart), StringComparison.OrdinalIgnoreCase))
                return "LTRIM";
            else if (String.Equals(functionName, nameof(SqlFunction.TrimEnd), StringComparison.OrdinalIgnoreCase))
                return "RTRIM";

            return base.GetBuildInFunctionName(functionName);
        }
    }
}
