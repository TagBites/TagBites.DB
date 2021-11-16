using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

#if MONOANDROID10_0
                Mono.Data.Sqlite.SqliteConnection.CreateFile(Database);
#elif !NETCOREAPP3_1
                Mono.Data.Sqlite.SqliteConnection.CreateFile(Database);
#else
                System.Data.SQLite.SQLiteConnection.CreateFile(Database);
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
