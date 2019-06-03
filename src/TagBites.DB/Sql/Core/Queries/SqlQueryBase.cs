using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TBS.Data.DB;

namespace TBS.Sql
{
    public abstract class SqlQueryBase : IQuerySource
    {
        protected internal abstract void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder);

        Query IQuerySource.CreateQuery(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            Accept(resolver, builder);
            return new Query(builder.Query, builder.Parameters);
        }


        public override string ToString()
        {
            var builder = SqlQueryBuilder.CreateToStringBuilder();
            Accept(SqlQueryResolver.DefaultToStringResolver, builder);
            return builder.Query;
        }
    }
}
