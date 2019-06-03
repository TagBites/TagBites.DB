using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql
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

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, null, builder);
        }
    }
}
