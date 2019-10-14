using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlConditionInOperator : SqlCondition
    {
        public SqlExpression Operand { get; }
        public IList Values { get; }

        public SqlConditionInOperator(SqlExpression operand, int[] values)
        {
            Guard.ArgumentNotNull(operand, nameof(operand));
            Guard.ArgumentNotNull(values, nameof(values));

            Operand = operand;
            Values = values;
        }
        public SqlConditionInOperator(SqlExpression operand, IList<SqlExpression> values)
        {
            Guard.ArgumentNotNull(operand, nameof(operand));
            Guard.ArgumentNotNullWithNotNullItems(values, nameof(values));

            Operand = operand;
            Values = values.ToList();
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }
        protected bool Equals(SqlConditionInOperator other)
        {
            return Equals(Operand, other.Operand) && Equals(Values, other.Values);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((SqlConditionInOperator)obj);
        }
        public override int GetHashCode()
        {
            return Operand.GetHashCode() ^ Values.GetHashCode();
        }
    }
}
