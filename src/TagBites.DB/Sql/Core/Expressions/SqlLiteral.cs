using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlLiteral : SqlExpression
    {
        public string Value { get; }

        public SqlLiteral(string value)
        {
            Guard.ArgumentNotNullOrEmpty(value, "value");
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
        public override string ToString()
        {
            return Value;
        }

        protected bool Equals(SqlLiteral other)
        {
            return string.Equals(Value, other.Value);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlLiteral)obj);
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
