using System.Collections.Generic;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlClauseValuesEntry : SqlClauseEntry
    {
        public IList<object> Values { get; }

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
