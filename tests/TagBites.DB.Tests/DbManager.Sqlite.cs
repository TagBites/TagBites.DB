using TagBites.DB;
using TagBites.DB.Sqlite;

namespace TagBites
{
    public static partial class DbManager
    {
        private const string SqliteDirectoryName = "sqlite";

        private static readonly string s_sqliteDirectory = PrepareSqliteDirectory();


        public static SqliteLinkProvider CreateSqliteProvider()
        {
            var database = Path.Combine(s_sqliteDirectory, $"{Guid.NewGuid():N}.sqlite");

            var arguments = new DbConnectionArguments
            {
                Database = database,

                UsePooling = true,
                MinPoolSize = 1,
                MaxPoolSize = 3
            };

            return new SqliteLinkProvider(arguments);
        }

        private static string PrepareSqliteDirectory()
        {
            var directory = Path.Combine(AppContext.BaseDirectory, SqliteDirectoryName);
            Directory.CreateDirectory(directory);

            // Clear files from previous run
            foreach (var file in Directory.GetFiles(directory, "*.sqlite"))
                try
                { File.Delete(file); }
                catch
                {
                    // ignored
                }

            return directory;
        }
    }
}
