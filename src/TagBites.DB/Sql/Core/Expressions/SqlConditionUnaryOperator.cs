﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlConditionUnaryOperator : SqlCondition
    {
        public SqlConditionUnaryOperatorType OperatorType { get; }
        public SqlExpression Operand { get; }

        public SqlConditionUnaryOperator(SqlConditionUnaryOperatorType operatorType, SqlExpression operand)
        {
            Guard.ArgumentNotNull(operand, nameof(operand));

            OperatorType = operatorType;
            Operand = operand;
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }

        protected bool Equals(SqlConditionUnaryOperator other)
        {
            return OperatorType == other.OperatorType && Equals(Operand, other.Operand);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlConditionUnaryOperator)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)OperatorType * 397) ^ Operand.GetHashCode();
            }
        }
    }
}
