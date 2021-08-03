using System;
using TagBites.DB;

namespace TagBites.Sql
{
    public abstract class SqlQueryBase : IQuerySource
    {
        public string TrackingComment { get; set; }


        protected internal abstract void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder);

        Query IQuerySource.CreateQuery(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            Accept(resolver, builder);
            return new Query(builder.Query, builder.Parameters);
        }

        public override string ToString() => ToString(SqlQueryResolver.DefaultToStringResolver);
        public string ToString(SqlQueryResolver queryResolver)
        {
            if (queryResolver == null)
                throw new ArgumentNullException(nameof(queryResolver));

            var builder = SqlQueryBuilder.CreateToStringBuilder();
            Accept(queryResolver, builder);
            return builder.Query;
        }
    }
}
