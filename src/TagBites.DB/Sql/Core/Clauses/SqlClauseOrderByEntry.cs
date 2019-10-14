using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlClauseOrderByEntry : SqlClauseEntry
    {
        public object Expression { get; }
        public SqlClauseOrderByEntryType OrderType { get; }
        public SqlClauseOrderByEntryNullsOrderType NullsOrderType { get; }

        public SqlClauseOrderByEntry(object expression, SqlClauseOrderByEntryType orderType)
        {
            Guard.ArgumentNotNull(expression, "expression");

            Expression = expression;
            OrderType = orderType;
        }
        public SqlClauseOrderByEntry(object expression, SqlClauseOrderByEntryType orderType, SqlClauseOrderByEntryNullsOrderType nullsOrderType)
            : this(expression, orderType)
        {
            NullsOrderType = nullsOrderType;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClauseEntry(this, builder);
        }
    }
}
