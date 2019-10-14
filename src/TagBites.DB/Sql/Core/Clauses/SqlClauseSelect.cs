using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlClauseSelect : SqlClauseCollectionBase<SqlExpressionWithAlias>
    {
        public SqlExpressionWithAlias Add(SqlExpression expression)
        {
            return base.Add(new SqlExpressionWithAlias(expression, null));
        }
        public SqlExpressionWithAlias Add(SqlExpression expression, string alias)
        {
            return base.Add(new SqlExpressionWithAlias(expression, alias));
        }

        public SqlExpressionWithAlias Add(SqlTable table, string columnName)
        {
            return Add(table.Column(columnName));
        }
        public SqlExpressionWithAlias Add(SqlTable table, string columnName, string alias)
        {
            return Add(table.Column(columnName), alias);
        }

        public SqlExpressionWithAlias AddLiteralExpression(string literalExpressionFormat, params SqlExpression[] args)
        {
            return Add(SqlExpression.LiteralExpression(literalExpressionFormat, args));
        }
        public SqlExpressionWithAlias AddLiteralExpression(string literalExpressionFormat, string alias, params SqlExpression[] args)
        {
            return Add(SqlExpression.LiteralExpression(literalExpressionFormat, args), alias);
        }

        public SqlExpressionWithAlias AddAll()
        {
            return Add(SqlExpression.Literal("*"));
        }
        public SqlExpressionWithAlias AddAll(SqlTable table)
        {
            return Add(SqlExpression.LiteralExpression("{0}.*", table));
        }

        public SqlExpression FirstOrDefault(string columnAlias)
        {
            return this.FirstOrDefault(x => x.Alias == columnAlias)?.Expression;
        }

        public bool Contains(string columnAlias)
        {
            return this.FirstOrDefault(x => x.Alias == columnAlias) != null;
        }
        public bool Contains(SqlExpression expression)
        {
            return this.FirstOrDefault(x => x.Expression.Equals(expression)) != null;
        }
        public bool Contains(SqlTable table, string columnName)
        {
            return Contains(table.Column(columnName));
        }

        public int IndexOf(string columnAlias)
        {
            for (var i = 0; i < Count; i++)
                if (this[i].Alias == columnAlias)
                    return i;

            return -1;
        }
        public int IndexOf(SqlExpression expression)
        {
            for (var i = 0; i < Count; i++)
                if (this[i].Expression.Equals(expression))
                    return i;

            return -1;
        }
        public int IndexOf(SqlTable table, string columnName)
        {
            return IndexOf(table.Column(columnName));
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, null, builder);
        }
    }
}
