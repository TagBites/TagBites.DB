using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlClauseGroupByEntry : SqlClauseEntry
    {
        public SqlExpression Expression { get; private set; }

        public SqlClauseGroupByEntry(SqlExpression expression)
        {
            Guard.ArgumentNotNull(expression, "expression");
            Expression = expression;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.Visit(Expression, builder);
        }
    }
}
