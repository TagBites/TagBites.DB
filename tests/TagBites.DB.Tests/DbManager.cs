using TagBites.DB;
using TagBites.DB.Npgsql;
using TagBites.DB.Postgres;
using TagBites.DB.SqlServer;

namespace TagBites
{
    public static partial class DbManager
    {
        public static PgSqlLinkProvider CreateNpgsqlProvider(bool pooling = true, int minPoolSize = 1, int maxPoolSize = 4)
        {
            var arguments = new DbConnectionArguments()
            {
                Host = "192.168.33.110",
                Port = 5434,
                Database = "newrr",
                Username = "axuser",
                Password = "abaxpmags",

                UsePooling = pooling,
                MinPoolSize = minPoolSize,
                MaxPoolSize = maxPoolSize
            };

            return new NpgsqlLinkProvider(arguments)
            {
                Configuration = { UseSystemTransactions = false, ImplicitCreateTransactionScopeIfNotExists = true }
            };
        }

        public static SqlServerLinkProvider CreateSqlServerProvider()
        {
            return new SqlServerLinkProvider("Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;");
        }
    }
}
