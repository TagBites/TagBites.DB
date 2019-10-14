using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlClauseSetEntry : SqlClauseEntry
    {
        public string ColumnName { get; }
        public SqlExpression Expression { get; }

        public SqlClauseSetEntry(string columnName, SqlExpression expression)
        {
            ColumnName = columnName;
            Expression = expression;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClauseEntry(this, builder);
        }
    }
}