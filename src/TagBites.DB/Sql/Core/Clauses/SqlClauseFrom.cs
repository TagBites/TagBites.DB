using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Data.DB;

namespace TBS.Sql
{
    public class SqlClauseFrom : SqlClauseCollectionBase<SqlClauseFromEntry>
    {
        public T Add<T>() where T : SqlTable, new()
        {
            return Add<T>(GetNextAlias());
        }
        public T Add<T>(string alias) where T : SqlTable, new()
        {
            var table = new T() { Alias = alias };
            Add(table);
            return table;
        }
        public SqlTable Add(SqlTable table)
        {
            Add(new SqlClauseFromEntry(table));
            return table;
        }

        public SqlTable Add(string tableName)
        {
            return Add(tableName, GetNextAlias());
        }
        public SqlTable Add(string tableName, string alias)
        {
            return Add(new SqlTable(tableName, alias));
        }

        public SqlTable Add(Query query)
        {
            return Add(query, GetNextAlias());
        }
        public SqlTable Add(Query query, string alias)
        {
            return Add(new SqlExpressionQuery(query), alias);
        }

        public SqlTable Add(SqlQueryBase query)
        {
            return Add(query, GetNextAlias());
        }
        public SqlTable Add(SqlQueryBase query, string alias)
        {
            return Add(query, alias, null);
        }
        public SqlTable Add(SqlQueryBase query, string alias, string[] columnNames)
        {
            var table = new SqlTable(query, alias);
            Add(new SqlClauseFromEntry(table, columnNames));
            return table;
        }

        public SqlTable Add(SqlClauseWithEntry withTable)
        {
            return Add(withTable, GetNextAlias());
        }
        public SqlTable Add(SqlClauseWithEntry withTable, string alias)
        {
            return Add(new SqlTable(withTable.Name, alias));
        }

        public SqlTable Add(SqlExpression queryExpression)
        {
            return Add(queryExpression, GetNextAlias());
        }
        public SqlTable Add(SqlExpression queryExpression, string alias)
        {
            return Add(queryExpression, alias, null);
        }
        public SqlTable Add(SqlExpression queryExpression, string alias, string[] columnNames)
        {
            if (queryExpression is SqlLiteral)
            {
                var table = new SqlTable((SqlLiteral)queryExpression, alias);
                Add(new SqlClauseFromEntry(table, columnNames));
                return table;
            }

            if (queryExpression is SqlLiteralExpression)
            {
                var table = new SqlTable((SqlLiteralExpression)queryExpression, alias);
                Add(new SqlClauseFromEntry(table, columnNames));
                return table;
            }
            if (queryExpression is SqlExpressionQuery)
            {
                var table = new SqlTable((SqlExpressionQuery)queryExpression, alias);
                Add(new SqlClauseFromEntry(table, columnNames));
                return table;
            }

            throw new ArgumentException("Invalid expression, query is required.", nameof(queryExpression));
        }

        private string GetNextAlias()
        {
            return $"tf_{Count + 1}";
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, null, builder);
        }
    }
}
