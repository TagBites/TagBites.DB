﻿using Npgsql;
using Npgsql.Logging;
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
        {
#if DEBUG
            NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug, true, true);
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
