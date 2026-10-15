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
            ColumnName = GetTargetColumn(target).ColumnName;
            Target = target;
            Expression = expression;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClauseEntry(this, builder);
        }

        private static SqlColumn GetTargetColumn(SqlExpression target)
        {
            return target switch
            {
                SqlColumn column => column,
                SqlExpressionIndexerOperator indexer => GetTargetColumn(indexer.Operand),
                SqlExpressionSliceOperator slice => GetTargetColumn(slice.Operand),
                SqlExpressionFieldOperator field => GetTargetColumn(field.Operand),
                null => throw new ArgumentNullException(nameof(target)),
                _ => throw new ArgumentException("Set target must be a column followed only by indexes, slices or fields.", nameof(target))
            };
        }
    }
}