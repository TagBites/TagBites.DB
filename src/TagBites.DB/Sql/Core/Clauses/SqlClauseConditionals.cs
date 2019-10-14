using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlClauseConditionals : SqlClauseCollectionBase<SqlCondition>
    {
        public void AddLiteralExpression(string literalExpressionFormat, params SqlExpression[] args)
        {
            Add(SqlExpression.ToCondition(SqlExpression.LiteralExpression(literalExpressionFormat, args)));
        }

        public void AddIsNull(SqlTable table, string columnName)
        {
            AddIsNull(table.Column(columnName));
        }
        public void AddIsNull(SqlExpression expression)
        {
            Add(SqlExpression.IsNull(expression));
        }

        public void AddIsNotNull(SqlTable table, string columnName)
        {
            AddIsNotNull(table.Column(columnName));
        }
        public void AddIsNotNull(SqlExpression expression)
        {
            Add(SqlExpression.IsNotNull(expression));
        }

        public void AddEquals(SqlExpression leftExpression, SqlExpression rightExpression)
        {
            Add(SqlExpression.AreEquals(leftExpression, rightExpression));
        }
        public void AddEquals(SqlTable leftTable, string leftTableColumnName, SqlExpression rightExpression)
        {
            Add(SqlExpression.AreEquals(leftTable.Column(leftTableColumnName), rightExpression));
        }
        public void AddEquals(SqlTable leftTable, string leftTableColumnName, SqlTable rightTable, string rightTableColumnName)
        {
            Add(SqlExpression.AreEquals(leftTable.Column(leftTableColumnName), rightTable.Column(rightTableColumnName)));
        }

        public void AddNotEquals(SqlExpression leftExpression, SqlExpression rightExpression)
        {
            Add(SqlExpression.AreNotEquals(leftExpression, rightExpression));
        }
        public void AddNotEquals(SqlTable leftTable, string leftTableColumnName, SqlExpression rightExpression)
        {
            Add(SqlExpression.AreNotEquals(leftTable.Column(leftTableColumnName), rightExpression));
        }
        public void AddNotEquals(SqlTable leftTable, string leftTableColumnName, SqlTable rightTable, string rightTableColumnName)
        {
            Add(SqlExpression.AreNotEquals(leftTable.Column(leftTableColumnName), rightTable.Column(rightTableColumnName)));
        }

        public void AddBetween(SqlExpression expression, SqlExpression minValueExpression, SqlExpression maxValueExpression)
        {
            Add(SqlExpression.Between(expression, minValueExpression, maxValueExpression));
        }
        public void AddBetween(SqlTable expressionTable, string expressionTableColumnName, SqlExpression minValueExpression, SqlExpression maxValueExpression)
        {
            Add(SqlExpression.Between(expressionTable.Column(expressionTableColumnName), minValueExpression, maxValueExpression));
        }

        public void AddIntervalIntersects(SqlExpression intervalAStart, SqlExpression intervalAEnd, SqlExpression intervalBStart, SqlExpression intervalBEnd)
        {
            Add(SqlExpression.IntervalIntersects(intervalAStart, intervalAEnd, intervalBStart, intervalBEnd));
        }
        public void AddIntervalCompleteIntersects(SqlExpression intervalAStart, SqlExpression intervalAEnd, SqlExpression intervalBStart, SqlExpression intervalBEnd)
        {
            Add(SqlExpression.IntervalCompleteIntersects(intervalAStart, intervalAEnd, intervalBStart, intervalBEnd));
        }
        public void AddIntervalLeftCompleteIntersects(SqlExpression intervalAStart, SqlExpression intervalAEnd, SqlExpression intervalBStart, SqlExpression intervalBEnd)
        {
            Add(SqlExpression.IntervalLeftCompleteIntersects(intervalAStart, intervalAEnd, intervalBStart, intervalBEnd));
        }

        public void AddIn(SqlTable table, string columnName, int[] values)
        {
            Add(SqlExpression.In(table.Column(columnName), values));
        }
        public void AddIn(SqlTable table, string columnName, params SqlExpression[] values)
        {
            Add(SqlExpression.In(table.Column(columnName), values));
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, null, builder);
        }
    }
}