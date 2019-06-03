using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlClauseUnionEntry : SqlClauseEntry
    {
        public SqlQuerySelect Select { get; private set; }
        public SqlClauseUnionEntryType Type { get; private set; }

        public SqlClauseUnionEntry(SqlQuerySelect select, SqlClauseUnionEntryType type)
        {
            Guard.ArgumentNotNull(select, "select");
            Select = select;
            Type = type;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClauseEntry(this, builder);
        }
    }

    public enum SqlClauseUnionEntryType
    {
        Default,
        All
    }
}
