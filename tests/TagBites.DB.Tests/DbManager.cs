using Npgsql;
using TagBites.DB.Npgsql;
using TagBites.DB.Postgres;
using TagBites.DB.SqlServer;

namespace TagBites
{
    public static partial class DbManager
    {
        public static PgSqlLinkProvider CreateNpgsqlProvider(bool pooling = true, int minPoolSize = 1, int maxPoolSize = 4)
        {
            var postgres = ConnectionSettings.Current.Postgres;
            var arguments = new NpgsqlConnectionStringBuilder()
            {
                Host = postgres.Host,
                Port = postgres.Port,
                Database = postgres.Database,
                Username = postgres.Username,
                Password = postgres.Password,

                Pooling = pooling,
                MinPoolSize = minPoolSize,
                MaxPoolSize = maxPoolSize,

                ArrayNullabilityMode = ArrayNullabilityMode.Always,
                SslMode = SslMode.Disable
            };

            return new NpgsqlLinkProvider(arguments.ToString())
            {
                Configuration = { UseSystemTransactions = false, ImplicitCreateTransactionScopeIfNotExists = true }
            };
        }

        public static SqlServerLinkProvider CreateSqlServerProvider()
        {
            return new SqlServerLinkProvider(ConnectionSettings.Current.SqlServerConnectionString);
        }
    }
}
