using System.Collections.Generic;
using System.Linq;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlConditionGroupOperator : SqlCondition
    {
        public SqlConditionGroupOperatorType OperatorType { get; }
        public IList<SqlCondition> Operands { get; }

        public SqlConditionGroupOperator(SqlConditionGroupOperatorType operatorType)
        {
            OperatorType = operatorType;
            Operands = EmptyConditionalArray;
        }
        public SqlConditionGroupOperator(SqlConditionGroupOperatorType operatorType, IList<SqlCondition> operands)
        {
            Guard.ArgumentNotNullWithNotNullItems(operands, nameof(operands));

            OperatorType = operatorType;
            Operands = operands;
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }

        protected bool Equals(SqlConditionGroupOperator other)
        {
            return OperatorType == other.OperatorType && Operands.SequenceEqual(other.Operands);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlConditionGroupOperator)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)OperatorType * 397) ^ Operands.GetHashCode();
            }
        }
    }
}
