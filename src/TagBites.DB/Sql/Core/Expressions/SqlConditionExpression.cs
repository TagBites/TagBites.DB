using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public sealed class SqlConditionExpression : SqlCondition
    {
        public SqlExpression Value { get; }

        internal SqlConditionExpression(SqlExpression value)
        {
            Guard.ArgumentNotNull(value, nameof(value));
            Value = value;
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }

        private bool Equals(SqlConditionExpression other)
        {
            return Equals(Value, other.Value);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj is SqlConditionExpression && Equals((SqlConditionExpression)obj);
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
