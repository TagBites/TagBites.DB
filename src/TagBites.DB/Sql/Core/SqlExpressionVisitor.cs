using System;
using System.Collections.Generic;
using System.Text;

namespace TBS.Sql
{
    public abstract class SqlExpressionVisitor : ISqlExpressionVisitor
    {
        public virtual void VisitExpression(SqlLiteral expression) { }
        public virtual void VisitExpression(SqlLiteralExpression expression)
        {
            foreach (var item in expression.Args)
                item.Accept(this);
        }
        public virtual void VisitExpression(SqlArgument expression) { }
        public virtual void VisitExpression(SqlTable expression) { }
        public virtual void VisitExpression(SqlColumn expression) => expression.Table.Accept(this);
        public virtual void VisitExpression(SqlExpressionFunctionCall expression)
        {
            foreach (var item in expression.Operants)
                item.Accept(this);
        }
        public virtual void VisitExpression(SqlExpressionWithAlias expression) => expression.Expression.Accept(this);
        public virtual void VisitExpression(SqlConditionExpression expression) => expression.Value.Accept(this);
        public virtual void VisitExpression(SqlConditionGroupOperator expression)
        {
            foreach (var item in expression.Operants)
                item.Accept(this);
        }
        public virtual void VisitExpression(SqlConditionBinaryOperator expression)
        {
            expression.OperandLeft.Accept(this);
            expression.OperandRight.Accept(this);
        }
        public virtual void VisitExpression(SqlConditionInOperator expression)
        {
            expression.Operand.Accept(this);

            foreach (var value in expression.Values)
                if (value is SqlExpression e)
                    e.Accept(this);
        }
        public virtual void VisitExpression(SqlConditionUnaryOperator expression) => expression.Operand.Accept(this);
        public virtual void VisitExpression(SqlExpressionBinaryOperator expression)
        {
            expression.OperandLeft.Accept(this);
            expression.OperandRight.Accept(this);
        }
        public virtual void VisitExpression(SqlExpressionCastOperator expression) => expression.Operand.Accept(this);
        public virtual void VisitExpression(SqlExpressionUnaryOperator expression) => expression.Operand.Accept(this);
        public virtual void VisitExpression(SqlExpressionIndexerOperator expression)
        {
            expression.Index.Accept(this);
            expression.Operand.Accept(this);
        }
        public virtual void VisitExpression(SqlExpressionSelect expression)
        {

        }
        public virtual void VisitExpression(SqlExpressionQuery expression)
        {
        }
    }
}
