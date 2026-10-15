using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlClauseSet : SqlClauseCollectionBase<SqlClauseSetEntry>
    {
        public void Add(string columnName, SqlExpression expression)
        {
            Add(new SqlClauseSetEntry(columnName, expression));
        }
        public void Add(string columnName, SqlQuerySelect query)
        {
            Add(new SqlClauseSetEntry(columnName, SqlExpression.ToExpression(query)));
        }
        public void Add(SqlExpression target, SqlExpression expression)
        {
            Add(new SqlClauseSetEntry(target, expression));
        }
        public void Add(SqlExpression target, SqlQuerySelect query)
        {
            Add(new SqlClauseSetEntry(target, SqlExpression.ToExpression(query)));
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, null, builder);
        }
    }
}
