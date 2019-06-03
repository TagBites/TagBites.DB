using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
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
