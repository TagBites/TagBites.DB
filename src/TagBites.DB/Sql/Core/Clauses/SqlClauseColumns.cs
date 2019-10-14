using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlClauseColumns : SqlClauseCollectionBase<string>
    {
        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, builder);
        }
    }
}
