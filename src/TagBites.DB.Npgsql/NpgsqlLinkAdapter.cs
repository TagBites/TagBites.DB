using System;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;
using TagBites.DB.Postgres;

namespace TagBites.DB.Npgsql
{
    public class NpgsqlLinkAdapter : PgSqlLinkAdapter
    {
        protected override DbLinkContext CreateDbLinkContext() => new NpgsqlLinkContext();

        protected override DbConnection CreateConnection(string connectionString)
        {
            var connection = new NpgsqlConnection(connectionString);
            return connection;
        }
        protected override string CreateConnectionString(DbConnectionArguments arguments)
        {
            var sb = new NpgsqlConnectionStringBuilder();

            foreach (var key in arguments.Keys)
                if (sb.ContainsKey(key))
                    sb[key] = arguments[key];

            sb.Host = arguments.Host;

            int port = arguments.Port;
            if (port > 0)
                sb.Port = port;

            sb.Database = arguments.Database;
            sb.Username = arguments.Username;
            sb.Password = arguments.Password;

            sb.Enlist = false;
            sb.Pooling = false;
            sb.MaxAutoPrepare = 0;

            sb.Remove(nameof(NpgsqlConnectionStringBuilder.MinPoolSize));
            sb.Remove(nameof(NpgsqlConnectionStringBuilder.MaxPoolSize));

            return sb.ToString();
        }

        protected override DbCommand CreateCommand(Query query)
        {
            var cmd = new NpgsqlCommand(query.Command);

            foreach (var item in query.Parameters)
            {
                var value = item.Value;
                cmd.Parameters.Add(new NpgsqlParameter(item.Name, value ?? DBNull.Value) { NpgsqlDbType = NpgsqlDbType.Unknown });
            }

            return cmd;
        }
        protected override DbDataAdapter CreateDataAdapter(DbCommand command)
        {
            return new NpgsqlDataAdapter((NpgsqlCommand)command);
        }
    }
}
