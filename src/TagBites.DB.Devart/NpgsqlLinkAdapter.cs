using System.Data.Common;
using Devart.Data.PostgreSql;
using TagBites.DB.Postgres;

namespace TagBites.DB.Npgsql
{
    public class NpgsqlLinkAdapter : PgSqlLinkAdapter
    {
        protected override DbLinkContext CreateDbLinkContext() => new NpgsqlLinkContext();

        protected override DbConnection CreateConnection(string connectionString)
        {
            var connection = new PgSqlConnection(connectionString);
            return connection;
        }
        protected override string CreateConnectionString(DbConnectionArguments arguments)
        {
            var sb = new PgSqlConnectionStringBuilder();

            foreach (var key in arguments.Keys)
                if (sb.ContainsKey(key))
                    sb[key] = arguments[key];

            sb.Host = arguments.Host;

            int port = arguments.Port;
            if (port > 0)
                sb.Port = port;

            sb.Database = arguments.Database;
            sb.UserId = arguments.Username ?? sb.UserId;
            sb.Password = arguments.Password;

            sb.Enlist = false;
            sb.Pooling = false;
            sb.UnpreparedExecute = true;

            sb.Remove(nameof(PgSqlConnectionStringBuilder.MinPoolSize));
            sb.Remove(nameof(PgSqlConnectionStringBuilder.MaxPoolSize));

            return sb.ToString();
        }

        protected override DbCommand CreateCommand(Query query)
        {
            var cmd = new PgSqlCommand(query.Command) { UnpreparedExecute = true };

            foreach (var item in query.Parameters)
            {
                var value = item.Value;
                cmd.Parameters.Add(new PgSqlParameter(item.Name, value));
            }

            return cmd;
        }
        protected override DbDataAdapter CreateDataAdapter(DbCommand command)
        {
            return new PgSqlDataAdapter((PgSqlCommand)command);
        }
    }
}
