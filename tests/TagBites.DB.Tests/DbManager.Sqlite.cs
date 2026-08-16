using TagBites.DB;
using TagBites.DB.Sqlite;

namespace TagBites
{
    public static partial class DbManager
    {
        public static SqliteLinkProvider CreateSqliteProvider()
        {
            var database = ConnectionSettings.Current.SqliteDatabase;
            if (!Path.IsPathRooted(database))
                database = Path.Combine(AppContext.BaseDirectory, database);

            if (File.Exists(database))
                File.Delete(database);

            var arguments = new DbConnectionArguments()
            {
                Database = database,

                UsePooling = true,
                MinPoolSize = 1,
                MaxPoolSize = 3
            };

            return new SqliteLinkProvider(arguments);
        }
    }
}
