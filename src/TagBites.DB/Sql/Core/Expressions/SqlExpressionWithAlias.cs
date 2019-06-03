using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlExpressionWithAlias : SqlExpression
    {
        public new SqlExpression Expression { get; }
        public string Alias { get; }

        public SqlExpressionWithAlias(SqlExpression expression, string alias)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            Expression = expression;
            Alias = alias;
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
            var text = base.ToString();

            if (!string.IsNullOrEmpty(Alias))
                text += $" AS {Alias}";

            return text;
        }

        protected bool Equals(SqlExpressionWithAlias other)
        {
            return Equals(Expression, other.Expression) && string.Equals(Alias, other.Alias);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlExpressionWithAlias)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Expression != null ? Expression.GetHashCode() : 0) * 397) ^ (Alias != null ? Alias.GetHashCode() : 0);
            }
        }
    }
}
