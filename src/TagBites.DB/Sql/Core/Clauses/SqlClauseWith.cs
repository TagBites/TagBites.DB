using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Data.DB;

namespace TBS.Sql
{
    public class SqlClauseWith : SqlClauseCollectionBase<SqlClauseWithEntry>
    {
        public SqlClauseWithEntry Add(SqlQueryBase query)
        {
            return Add(GetNextName(), null, query);
        }
        public SqlClauseWithEntry Add(string[] columnNames, SqlQueryBase query)
        {
            return Add(GetNextName(), columnNames, query);
        }

        public SqlClauseWithEntry Add(SqlExpression select)
        {
            return Add(GetNextName(), null, select);
        }
        public SqlClauseWithEntry Add(string[] columnNames, SqlExpression select)
        {
            return Add(GetNextName(), columnNames, select);
        }

        public SqlClauseWithEntry Add(Query select)
        {
            return Add(GetNextName(), null, select);
        }
        public SqlClauseWithEntry Add(string[] columnNames, Query select)
        {
            return Add(GetNextName(), columnNames, select);
        }

        public SqlClauseWithEntry Add(string withName, SqlQueryBase query)
        {
            return Add(withName, null, query);
        }
        public SqlClauseWithEntry Add(string withName, string[] columnNames, SqlQueryBase query)
        {
            return Add(new SqlClauseWithEntry(withName, columnNames, query));
        }

        public SqlClauseWithEntry Add(string withName, SqlExpression select)
        {
            return Add(withName, null, select);
        }
        public SqlClauseWithEntry Add(string withName, string[] columnNames, SqlExpression select)
        {
            if (select is SqlLiteral)
                return Add(new SqlClauseWithEntry(withName, columnNames, (SqlLiteral)select));

            if (select is SqlLiteralExpression)
                return Add(new SqlClauseWithEntry(withName, columnNames, (SqlLiteralExpression)select));

            throw new ArgumentException("Invalid expression, query is required.", nameof(select));
        }

        public SqlClauseWithEntry Add(string withName, Query query)
        {
            return Add(withName, null, query);
        }
        public SqlClauseWithEntry Add(string withName, string[] columnNames, Query query)
        {
            return Add(new SqlClauseWithEntry(withName, columnNames, new SqlExpressionQuery(query)));
        }

        private string GetNextName()
        {
            return $"TW{Count + 1}";
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, null, builder);
        }
    }
}
