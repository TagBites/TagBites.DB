using Npgsql;

namespace TBS.Data.DB.PostgreSql
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
                x[nameof(NpgsqlConnectionStringBuilder.TcpKeepAliveTime)] = "1";
                x[nameof(NpgsqlConnectionStringBuilder.TcpKeepAliveInterval)] = "50";
                x[nameof(NpgsqlConnectionStringBuilder.Pooling)] = "false";
            });
        }
    }
}
