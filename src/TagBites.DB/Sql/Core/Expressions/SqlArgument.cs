using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlArgument : SqlExpression
    {
        public object Value { get; }

        public SqlArgument(object value)
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

        private bool Equals(SqlArgument other)
        {
            return Equals(Value, other.Value);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlArgument)obj);
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
