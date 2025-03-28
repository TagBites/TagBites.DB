using System.Data.Common;
using TagBites.Sql;
using TagBites.Sql.Sqlite;

#if NUGET
using System.Data.SQLite;
#else
using Mono.Data.Sqlite;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteConnectionStringBuilder = Mono.Data.Sqlite.SqliteConnectionStringBuilder;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteDataAdapter = Mono.Data.Sqlite.SqliteDataAdapter;
#endif


namespace TagBites.DB.Sqlite
{
    public class SqliteLinkAdapter : DbLinkAdapter
    {
        public override SqlQueryResolver QueryResolver { get; } = new SqliteQueryResolver();
        public override int DefaultPort => 0;


        protected override DbConnection CreateConnection(string connectionString)
        {
            var connection = new SQLiteConnection(connectionString);
            return connection;
        }
        protected override string CreateConnectionString(DbConnectionArguments arguments)
        {
            var sb = new SQLiteConnectionStringBuilder
            {
                Pooling = false,
                SyncMode = SynchronizationModes.Full
            };
            sb["busytimeout"] = 1000;

            foreach (var key in arguments.Keys)
                sb[key] = arguments[key];

            sb.DataSource = arguments.Database;
            sb.Password = arguments.Password;

            return sb.ToString();
        }

        protected override DbCommand CreateCommand(Query query)
        {
            var cmd = new SQLiteCommand(query.Command);

            foreach (var item in query.Parameters)
            {
                var value = item.Value;
                cmd.Parameters.Add(new SQLiteParameter(item.Name, value));
            }

            //cmd.Prepare();
            return cmd;
        }
        protected override DbDataAdapter CreateDataAdapter(DbCommand command)
        {
            return new SQLiteDataAdapter((SQLiteCommand)command);
        }
    }
}
