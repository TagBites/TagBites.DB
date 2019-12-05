using Devart.Data.PostgreSql;
using TagBites.DB.Postgres;

namespace TagBites.DB.Npgsql
{
    public class NpgsqlLinkProvider : PgSqlLinkProvider
    {
        public NpgsqlLinkProvider(string connectionString)
            : this(new DbConnectionArguments(connectionString))
        { }
        public NpgsqlLinkProvider(DbConnectionArguments arguments)
            : base(new NpgsqlLinkAdapter(), arguments)
        { }


        protected override PgSqlLink CreateExclusiveNotifyLink()
        {
            return (PgSqlLink)CreateExclusiveLink(x =>
            {
                x[nameof(PgSqlConnectionStringBuilder.KeepAlive)] = "0";
                x[nameof(PgSqlConnectionStringBuilder.Pooling)] = "false";
            });
        }
    }
}
