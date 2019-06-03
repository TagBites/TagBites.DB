using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TBS.Utils;

namespace TBS.DB.Entity
{
    public class EntityQuery<TSource> : IOrderedQueryable<TSource>
    {
        public Type ElementType => typeof(TSource);
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }

        public EntityQuery(IQueryProvider provider, IQueryable<TSource> innerSource)
        {
            Guard.ArgumentNotNull(provider, nameof(provider));
            Guard.ArgumentNotNull(innerSource, nameof(innerSource));

            Provider = provider;
            Expression = Expression.Constant(innerSource);
        }
        public EntityQuery(IQueryProvider provider, Expression expression = null)
        {
            Guard.ArgumentNotNull(provider, nameof(provider));

            Provider = provider;
            Expression = expression ?? Expression.Constant(this);
        }


        public IEnumerator<TSource> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<TSource>>(Expression).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
