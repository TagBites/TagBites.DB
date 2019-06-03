using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql
{
    public class SqlClauseValues : SqlClauseCollectionBase<SqlClauseValuesEntry>
    {
        public SqlClauseValuesEntry Add(params object[] values)
        {
            return base.Add(new SqlClauseValuesEntry((IList<object>)values));
        }
        public SqlClauseValuesEntry Add(IList<object> values)
        {
            return base.Add(new SqlClauseValuesEntry(values));
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, null, builder);
        }
    }
}
