using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.Sql
{
    public abstract class SqlQueryInsertBase : SqlQueryBase
    {
        public SqlClauseWith With { get; } = new SqlClauseWith();
        public SqlTable Into { get; }
        public SqlClauseColumns Columns { get; } = new SqlClauseColumns();
        public SqlClauseReturningForInsert Returning { get; } = new SqlClauseReturningForInsert();

        protected SqlQueryInsertBase(string intoTableName, string intoTableNameAlias)
        {
            Guard.ArgumentNotNullOrEmpty(intoTableName, nameof(intoTableName));
            Into = new SqlTable(intoTableName, string.IsNullOrEmpty(intoTableNameAlias) ? "iit_0" : intoTableNameAlias);
        }
    }
}
