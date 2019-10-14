using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlClauseUnion : SqlClauseCollectionBase<SqlClauseUnionEntry>
    {
        public SqlClauseUnionEntry Add(SqlQuerySelect table, SqlClauseUnionEntryType type)
        {
            return Add(new SqlClauseUnionEntry(table, type));
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, builder);
        }
    }
}
