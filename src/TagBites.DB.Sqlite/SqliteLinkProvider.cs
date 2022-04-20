using System.IO;

namespace TagBites.DB.Sqlite
{
    public class SqliteLinkProvider : DbLinkProvider
    {
        public SqliteLinkProvider(string connectionString)
            : this(new DbConnectionArguments(connectionString))
        { }
        public SqliteLinkProvider(DbConnectionArguments arguments)
            : this(new SqliteLinkAdapter(), arguments)
        { }
        public SqliteLinkProvider(SqliteLinkAdapter adapter, string connectionString)
            : this(adapter, new DbConnectionArguments(connectionString))
        { }
        public SqliteLinkProvider(SqliteLinkAdapter adapter, DbConnectionArguments arguments)
            : base(adapter, arguments)
        {
            if (!File.Exists(Database))
            {
                var directory = Path.GetDirectoryName(Database);
                if (directory != null)
                    Directory.CreateDirectory(directory);

#if NUGET
                System.Data.SQLite.SQLiteConnection.CreateFile(Database);
#else
                Mono.Data.Sqlite.SqliteConnection.CreateFile(Database);
#endif
            }
        }


        public DbQueryCursorManager CreateCursorManager()
        {
            return (DbQueryCursorManager)CreateCursorManagerInner();
        }
        protected override IDbCursorManager CreateCursorManagerInner()
        {
            return new DbQueryCursorManager(this);
        }
    }
}
