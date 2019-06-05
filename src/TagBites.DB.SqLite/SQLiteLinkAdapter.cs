using System.Data.Common;
using System.Data.SQLite;
using TBS.Sql;
using TBS.Sql.SQLite;

namespace TBS.Data.DB.SQLite
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteLinkAdapter : DbLinkAdapter
    {
        public override SqlQueryResolver QueryResolver { get; } = new SQLiteQueryResolver();
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
                SyncMode = SynchronizationModes.Full,
                BusyTimeout = 1000
            };

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
                //if (value != null)
                //{
                //    Type valueType = value.GetType();
                //    if (valueType.IsClass && valueType != typeof(string) && !valueType.IsArray)
                //    {
                //        var args = TBS.Utils.TypeUtils.GetGenericArguments(valueType, typeof(IEnumerable<>));
                //        if (args != null && args.Length == 1)
                //            value = ((IEnumerable)value).Cast<object>().ToArray();
                //    }
                //}

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
