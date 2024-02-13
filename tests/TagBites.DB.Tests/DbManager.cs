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
            var arguments = new NpgsqlConnectionStringBuilder()
            {
                Host = "192.168.33.110",
                Port = 5434,
                Database = "newrr",
                Username = "axuser",
                Password = "abaxpmags",

                Pooling = pooling,
                MinPoolSize = minPoolSize,
                MaxPoolSize = maxPoolSize,

                ArrayNullabilityMode = ArrayNullabilityMode.Always,
                SslMode = SslMode.Allow
            };

            return new NpgsqlLinkProvider(arguments.ToString())
            {
                Configuration = { UseSystemTransactions = false, ImplicitCreateTransactionScopeIfNotExists = true }
            };
        }

        public static SqlServerLinkProvider CreateSqlServerProvider()
        {
            return new SqlServerLinkProvider("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
        }
    }
}
