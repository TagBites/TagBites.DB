using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlClauseValuesEntry : SqlClauseEntry
    {
        public IList<object> Values { get; private set; }

        public SqlClauseValuesEntry(IList<object> values)
        {
            Guard.ArgumentNotNullOrEmpty(values, nameof(values));
            Values = values;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClauseEntry(this, builder);
        }
    }
}
