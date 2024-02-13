using Microsoft.Extensions.Logging;
using Npgsql;
using TagBites.DB.Postgres;

namespace TagBites.DB.Npgsql
{
    public class NpgsqlLinkProvider : PgSqlLinkProvider
    {
#if DEBUG
        private static readonly object s_npgsqlLogManagerSyncRoot = new();
#endif

        public NpgsqlLinkProvider(string connectionString)
            : this(new DbConnectionArguments(connectionString))
        { }
        public NpgsqlLinkProvider(DbConnectionArguments arguments)
            : base(new NpgsqlLinkAdapter(), arguments)
        {
#if DEBUG
            lock (s_npgsqlLogManagerSyncRoot)
            {
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                NpgsqlLoggingConfiguration.InitializeLogging(loggerFactory, true);
            }
#endif
        }


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
