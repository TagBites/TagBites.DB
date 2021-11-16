#if SQLITE
using TagBites.DB;
using TagBites.DB.Sqlite;

namespace TagBites
{
    public static partial class DbManager
    {
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

#endif
