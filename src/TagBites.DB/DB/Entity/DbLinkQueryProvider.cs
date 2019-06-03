using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TBS.Data.DB;
using TBS.Sql;

namespace TBS.DB.Entity
{
    public class DbLinkQueryProvider : QueryProvider, IDbLinkEntityQueryProvider
    {
        internal Action<SqlQuerySelect> SqlQueryGenerated;

        private readonly IDbLink m_link;

        public DbLinkQueryProvider(IDbLink dbLink)
        {
            m_link = dbLink;
        }


        public SqlQuerySelect GetQuery(Expression expression)
        {
            return new QueryTranslator().Translate(expression);
        }
        public override TResult Execute<TResult>(Expression expression)
        {
            var isCollection = typeof(TResult).GetTypeInfo().IsGenericType &&
                typeof(TResult).GetGenericTypeDefinition() == typeof(IEnumerable<>);
            var itemType = isCollection
                ? typeof(TResult).GetTypeInfo().GenericTypeArguments.Single()
                : typeof(TResult);

            var m_translator = new QueryTranslator();
            var sqlQuerySelect = m_translator.Translate(expression);

            SqlQueryGenerated?.Invoke(sqlQuerySelect);

            // Executed local
            if (m_translator.LocalExecutionInfo != null)
            {
                var queryResult = m_link.Execute(sqlQuerySelect);
                var localMethodType = m_translator.LocalExecutionInfo.LocalType;
                var collection = CreateResult(localMethodType, queryResult, m_translator.Initializer);
                var parameters = new object[] { collection };
                var methodParameters = m_translator.LocalExecutionInfo.Parameters;
                if (methodParameters?.Length > 0)
                    parameters = parameters.Concat(methodParameters).ToArray();
                var ret = m_translator.LocalExecutionInfo.Method.Invoke(collection, parameters);
                return (TResult)ret;
            }
            else if (isCollection)
            {
                var result = m_link.Execute(sqlQuerySelect);
                var collection = CreateResult(itemType, result, m_translator.Initializer);
                return (TResult)collection;
            }
            else if (!itemType.GetTypeInfo().IsPrimitive && !itemType.GetTypeInfo().IsValueType && (itemType != typeof(string)))
            {
                // TODO
                throw new NotImplementedException();
            }
            else
            {
                return m_link.ExecuteScalar<TResult>(sqlQuerySelect);
            }
        }
        public override object Execute(Expression expression)
        {
            var m_translator = new QueryTranslator();
            var sqlQuerySelect = m_translator.Translate(expression);
            return m_link.Execute(sqlQuerySelect);
        }

        private IList CreateResult(Type entityType, QueryResult dataProvider, QueryObjectInitializer initializer)
        {
            return (IList)GetType().GetTypeInfo().GetDeclaredMethod(nameof(CreateResultCore))
                .MakeGenericMethod(entityType)
                .Invoke(this, new object[] { dataProvider, initializer });
        }
        private QueryObjectResult<T> CreateResultCore<T>(QueryResult dataProvider, QueryObjectInitializer initializer)
        {
            return new QueryObjectResult<T>(dataProvider, initializer);
        }
    }
}
