using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql
{
    public class SqlClauseOrderBy : SqlClauseCollectionBase<SqlClauseOrderByEntry>
    {
        public void Add(SqlExpression expression, SqlClauseOrderByEntryType order)
        {
            Add(new SqlClauseOrderByEntry(expression, order));
        }
        public void Add(SqlExpression expression, SqlClauseOrderByEntryType order, SqlClauseOrderByEntryNullsOrderType nullsOrder)
        {
            Add(new SqlClauseOrderByEntry(expression, order, nullsOrder));
        }

        public void Add(SqlTable table, string columnName, SqlClauseOrderByEntryType order)
        {
            Add(table.Column(columnName), order);
        }
        public void Add(SqlTable table, string columnName, SqlClauseOrderByEntryType order, SqlClauseOrderByEntryNullsOrderType nullsOrder)
        {
            Add(table.Column(columnName), order, nullsOrder);
        }

        public void Insert(int index, SqlExpression expression, SqlClauseOrderByEntryType order)
        {
            Insert(index, new SqlClauseOrderByEntry(expression, order));
        }
        public void Insert(int index, SqlExpression expression, SqlClauseOrderByEntryType order, SqlClauseOrderByEntryNullsOrderType nullsOrder)
        {
            Insert(index, new SqlClauseOrderByEntry(expression, order, nullsOrder));
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, null, builder);
        }
    }
}
