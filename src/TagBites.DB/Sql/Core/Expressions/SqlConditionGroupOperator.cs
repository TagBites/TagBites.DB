using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Collections.ObjectModel;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlConditionGroupOperator : SqlCondition
    {
        public SqlConditionGroupOperatorType OperatorType { get; }
        public IList<SqlCondition> Operants { get; }

        public SqlConditionGroupOperator(SqlConditionGroupOperatorType operatorType)
        {
            OperatorType = operatorType;
            Operants = EmptyConditionalArray;
        }
        public SqlConditionGroupOperator(SqlConditionGroupOperatorType operatorType, IList<SqlCondition> operants)
        {
            Guard.ArgumentNotNullWithNotNullItems(operants, nameof(operants));

            OperatorType = operatorType;
            Operants = operants;
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
            return OperatorType == other.OperatorType && Operants.SequenceEqual(other.Operants);
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
                return ((int)OperatorType * 397) ^ Operants.GetHashCode();
            }
        }
    }
}
