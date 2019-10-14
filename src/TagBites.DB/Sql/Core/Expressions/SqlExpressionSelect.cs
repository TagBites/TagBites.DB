using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlExpressionSelect : SqlExpression
    {
        public SqlQuerySelect Select { get; }

        public SqlExpressionSelect(SqlQuerySelect select)
        {
            Guard.ArgumentNotNull(select, nameof(select));
            Select = select;
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }

        protected bool Equals(SqlExpressionSelect other)
        {
            return Equals(Select, other.Select);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlExpressionSelect)obj);
        }
        public override int GetHashCode()
        {
            return Select.GetHashCode();
        }
    }
}
