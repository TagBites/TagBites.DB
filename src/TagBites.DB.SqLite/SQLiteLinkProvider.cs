using System.Data.SQLite;
using System.IO;

namespace TBS.Data.DB.SQLite
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteLinkProvider : DbLinkProvider
    {
        public SQLiteLinkProvider(string connectionString)
            : this(new DbConnectionArguments(connectionString))
        { }
        public SQLiteLinkProvider(DbConnectionArguments arguments)
            : this(new SQLiteLinkAdapter(), arguments)
        { }
        public SQLiteLinkProvider(SQLiteLinkAdapter adapter, string connectionString)
            : this(adapter, new DbConnectionArguments(connectionString))
        { }
        public SQLiteLinkProvider(SQLiteLinkAdapter adapter, DbConnectionArguments arguments)
            : base(adapter, arguments)
        {
            if (!File.Exists(Database))
            {
                var directory = Path.GetDirectoryName(Database);
                if (directory != null)
                    Directory.CreateDirectory(directory);

                SQLiteConnection.CreateFile(Database);
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
