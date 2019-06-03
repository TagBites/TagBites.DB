using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace TBS.Data.DB.SQLite
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteLinkProvider : DbLinkProvider
    {
        public SQLiteLinkProvider(string connectionString)
            : base(new SQLiteLinkAdapter(), connectionString)
        {
            if (!File.Exists(Database))
            {
                var directory = Path.GetDirectoryName(Database);
                if (directory != null)
                    Directory.CreateDirectory(directory);

                SQLiteConnection.CreateFile(Database);
            }
        }
        public SQLiteLinkProvider(SQLiteLinkAdapter adapter, string connectionString)
            : base(adapter, connectionString)
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

