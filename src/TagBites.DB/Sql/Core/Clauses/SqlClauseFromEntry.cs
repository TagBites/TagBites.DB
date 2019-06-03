using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlClauseFromEntry : SqlClauseEntry
    {
        public SqlTable Table { get; }
        public string[] ColumnNames { get; }

        public SqlClauseFromEntry(SqlTable table)
        {
            Guard.ArgumentNotNull(table, nameof(table));
            Table = table;
            ColumnNames = Array.Empty<string>();
        }
        public SqlClauseFromEntry(SqlTable table, string[] columnNames)
        {
            Guard.ArgumentNotNull(table, nameof(table));
            Table = table;
            ColumnNames = columnNames ?? Array.Empty<string>();
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClauseEntry(this, builder);
        }
    }
}
