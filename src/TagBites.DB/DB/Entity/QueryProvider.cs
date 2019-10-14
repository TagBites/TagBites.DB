using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace TagBites.DB.Entity
{
    public abstract class QueryProvider : IQueryProvider
    {
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new EntityQuery<TElement>(this, expression);
        }
        public IQueryable CreateQuery(Expression expression)
        {
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(EntityQuery<>).MakeGenericType(expression.Type), new object[] { this, expression });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public abstract TResult Execute<TResult>(Expression expression);
        public abstract object Execute(Expression expression);
    }
}
