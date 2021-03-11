using TagBites.DB.Npgsql;
using TagBites.DB.Postgres;
using TagBites.DB.SqLite;

namespace TagBites.DB.Tests.DB.Core
{
    public static class DbManager
    {
        public static PgSqlLinkProvider CreateNpgsqlProvider(bool pooling = true, int minPoolSize = 1, int maxPoolSize = 3)
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

        // ReSharper disable once InconsistentNaming
        public static SqliteLinkProvider CreateSQLiteProvider()
        {
            var arguments = new DbConnectionArguments()
            {
                Database = "D:/db.sqlite",

                UsePooling = true,
                MinPoolSize = 1,
                MaxPoolSize = 3
            };

            return new SqliteLinkProvider(arguments);
        }
    }
}
