using System;
using System.Collections.Generic;
using System.Text;

namespace TBS.Sql
{
    public interface ISqlExpressionVisitor
    {
        void VisitExpression(SqlLiteral expression);
        void VisitExpression(SqlLiteralExpression expression);
        void VisitExpression(SqlArgument expression);
        void VisitExpression(SqlTable expression);
        void VisitExpression(SqlColumn expression);
        void VisitExpression(SqlExpressionFunctionCall expression);
        void VisitExpression(SqlExpressionWithAlias expression);
        void VisitExpression(SqlConditionExpression expression);
        void VisitExpression(SqlConditionGroupOperator expression);
        void VisitExpression(SqlConditionBinaryOperator expression);
        void VisitExpression(SqlConditionInOperator expression);
        void VisitExpression(SqlConditionUnaryOperator expression);
        void VisitExpression(SqlExpressionBinaryOperator expression);
        void VisitExpression(SqlExpressionCastOperator expression);
        void VisitExpression(SqlExpressionUnaryOperator expression);
        void VisitExpression(SqlExpressionIndexerOperator expression);
        void VisitExpression(SqlExpressionSelect expression);
        void VisitExpression(SqlExpressionQuery expression);
    }
}
