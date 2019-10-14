using System.Collections.Generic;

namespace TagBites.Sql
{
    public class SqlClauseValues : SqlClauseCollectionBase<SqlClauseValuesEntry>
    {
        public SqlClauseValuesEntry Add(params object[] values)
        {
            return base.Add(new SqlClauseValuesEntry(values));
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
