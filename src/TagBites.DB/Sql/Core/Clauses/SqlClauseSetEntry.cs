using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlClauseSetEntry : SqlClauseEntry
    {
        public string ColumnName { get; }
        public SqlExpression Target { get; }
        public SqlExpression Expression { get; }

        public SqlClauseSetEntry(string columnName, SqlExpression expression)
        {
            ColumnName = columnName;
            Expression = expression;
        }
        public SqlClauseSetEntry(SqlExpression target, SqlExpression expression)
        {
            ColumnName = SqlColumn.GetTargetColumn(target).ColumnName;
            Target = target;
            Expression = expression;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClauseEntry(this, builder);
        }
    }
}