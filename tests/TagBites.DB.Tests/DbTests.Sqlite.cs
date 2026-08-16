using TagBites.DB.Sqlite;

namespace TagBites
{
    partial class DbTests
    {
        private SqliteLinkProvider _sqliteProvider;

        public SqliteLinkProvider SqliteProvider
        {
            get
            {
                lock (m_locker)
                {
                    if (_sqliteProvider == null)
                        _sqliteProvider = DbManager.CreateSqliteProvider();

                    return _sqliteProvider;
                }
            }
        }
    }
}
