using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql
{
    public abstract class SqlClauseEntry : ISqlElement
    {
        void ISqlElement.Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            Accept(resolver, builder);
        }
        protected abstract void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder);

        public override string ToString()
        {
            var builder = SqlQueryBuilder.CreateToStringBuilder();
            Accept(SqlQueryResolver.DefaultToStringResolver, builder);
            return builder.Query;
        }
    }
}
