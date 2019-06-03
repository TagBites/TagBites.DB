using Npgsql;
using System.Data.SQLite;
using TBS.Data.DB.PostgreSql;
using TBS.Data.DB.SQLite;

namespace TBS.Data.UnitTests
{
    public static class DbManager
    {
        public static PgSqlLinkProvider CreateNpgsqlProvider(bool pooling = true, int minPoolSize = 1, int maxPoolSize = 3)
        {
            var csb = new NpgsqlConnectionStringBuilder();
            csb.Pooling = pooling;
            csb.MinPoolSize = minPoolSize;
            csb.MaxPoolSize = maxPoolSize;
            csb.Enlist = false;
            csb.ServerCompatibilityMode = ServerCompatibilityMode.NoTypeLoading;

            if (true)
            {
                csb.Host = "192.168.50.1";
                csb.Port = 5432;
                csb.Database = "newrr";
                csb.Username = "tomasz";
                csb.Password = "tomasz";
            }
            else
            {
                csb.Host = "127.0.0.1";
                csb.Port = 5432;
                csb.Database = "prezent";
                csb.Username = "postgres";
                csb.Password = "baza";
            }

            return new NpgsqlLinkProvider(csb.ToString())
            {
                Configuration = { UseSystemTransactions = true, ImplicitCreateTransactionScopeIfNotExists = true }
            };
        }

        // ReSharper disable once InconsistentNaming
        public static SQLiteLinkProvider CreateSQLiteProvider()
        {
            var csb = new SQLiteConnectionStringBuilder();
            csb.DataSource = "D:/db.sqlite";
            csb.Pooling = false;
            csb.SyncMode = SynchronizationModes.Full;
            csb.BusyTimeout = 1000;
            csb.Add("MinPoolSize", 1);
            csb.Add("MaxPoolSize", 10);

            return new SQLiteLinkProvider(csb.ToString());
        }
    }
}
