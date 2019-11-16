using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TagBites.DB.Entity
{
    public class EntityQueryable<TResult> : IOrderedQueryable<TResult>
    {
        public Type ElementType => typeof(TResult);
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }

        public EntityQueryable(IQueryProvider provider, IQueryable<TResult> innerSource)
        {
            //Guard.ArgumentNotNull(provider, nameof(provider));
            // Guard.ArgumentNotNull(innerSource, nameof(innerSource));

            Provider = provider;
            Expression = Expression.Constant(innerSource);
        }
        public EntityQueryable(IQueryProvider provider, Expression expression = null)
        {
            //  Guard.ArgumentNotNull(provider, nameof(provider));

            Provider = provider;
            Expression = expression ?? Expression.Constant(this);
        }


        public IEnumerator<TResult> GetEnumerator() => Provider.Execute<IEnumerable<TResult>>(Expression).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
