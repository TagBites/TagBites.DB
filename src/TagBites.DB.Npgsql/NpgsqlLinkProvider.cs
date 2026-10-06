using Npgsql;
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
                x[nameof(NpgsqlConnectionStringBuilder.KeepAlive)] = "0";
                x[nameof(NpgsqlConnectionStringBuilder.TcpKeepAlive)] = "true";
                x[nameof(NpgsqlConnectionStringBuilder.TcpKeepAliveTime)] = "30";
                x[nameof(NpgsqlConnectionStringBuilder.TcpKeepAliveInterval)] = "5";
                x[nameof(NpgsqlConnectionStringBuilder.Pooling)] = "false";
                // The connection only receives notifications, so it skips the query that loads the database types
                x[nameof(NpgsqlConnectionStringBuilder.ServerCompatibilityMode)] = nameof(ServerCompatibilityMode.NoTypeLoading);
            }, x => x[PgSqlBagKeys.IsNotifyContext] = true, excludeFromPool: true);
        }
    }
}
