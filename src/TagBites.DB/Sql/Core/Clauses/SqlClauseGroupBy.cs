using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlClauseGroupBy : SqlClauseCollectionBase<SqlClauseGroupByEntry>
    {
        public void Add(SqlExpression expression)
        {
            Add(new SqlClauseGroupByEntry(expression));
        }

        public void Add(SqlTable table, string columnName)
        {
            Add(SqlExpression.Column(table, columnName));
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, null, builder);
        }
    }
}
