using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlExpressionBinaryOperator : SqlExpression
    {
        public SqlExpressionBinaryOperatorType OperatorType { get; }
        public SqlExpression OperandLeft { get; }
        public SqlExpression OperandRight { get; }

        public SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType operatorType, SqlExpression operandLeft, SqlExpression operandRight)
        {
            Guard.ArgumentNotNull(operandLeft, nameof(operandLeft));
            Guard.ArgumentNotNull(operandRight, nameof(operandRight));

            OperatorType = operatorType;
            OperandLeft = operandLeft;
            OperandRight = operandRight;
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }

        protected bool Equals(SqlExpressionBinaryOperator other)
        {
            return OperatorType == other.OperatorType && Equals(OperandLeft, other.OperandLeft) && Equals(OperandRight, other.OperandRight);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlExpressionBinaryOperator)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)OperatorType;
                hashCode = (hashCode * 397) ^ OperandLeft.GetHashCode();
                hashCode = (hashCode * 397) ^ OperandRight.GetHashCode();
                return hashCode;
            }
        }
    }
}
