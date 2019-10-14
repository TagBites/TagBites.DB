using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace TagBites.DB.SqLite
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
