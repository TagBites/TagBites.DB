using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.DB;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlExpressionQuery : SqlExpression
    {
        public Query Query { get; }

        internal SqlExpressionQuery(Query query)
        {
            Guard.ArgumentNotNull(query, nameof(query));
            Query = query;
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }

        protected bool Equals(SqlExpressionQuery other)
        {
            return Equals(Query, other.Query);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlExpressionQuery)obj);
        }
        public override int GetHashCode()
        {
            return Query.GetHashCode();
        }
    }
}
