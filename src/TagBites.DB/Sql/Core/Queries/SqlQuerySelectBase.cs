using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql
{
    public abstract class SqlQuerySelectBase : SqlQueryBase
    {
        public SqlClauseOrderBy OrderBy { get; } = new SqlClauseOrderBy();
        public SqlClauseUnion Union { get; } = new SqlClauseUnion();

        public int? Offset { get; set; }
        public int? Limit { get; set; }
    }
}