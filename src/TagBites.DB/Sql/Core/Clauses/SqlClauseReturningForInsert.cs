using System;
using System.Collections.Generic;
using System.Text;

namespace TBS.Sql
{
    public class SqlClauseReturningForInsert : SqlClauseSelect
    {
        public void Add(string column, string alias)
        {
            Add(SqlExpression.Literal(SqlQueryResolver.QuoteIdentifierIfNeeded(column)), alias);
        }
    }
}
