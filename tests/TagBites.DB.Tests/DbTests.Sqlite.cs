#if SQLITE

using TagBites.DB.Sqlite;

namespace TagBites
{
    partial class DbTests
    {
        private SqliteLinkProvider _mSqliteProvider;

        public SqliteLinkProvider SqliteProvider
        {
            get
            {
                lock (m_locker)
                {
                    if (_mSqliteProvider == null)
                        _mSqliteProvider = DbManager.CreateSQLiteProvider();

                    return _mSqliteProvider;
                }
            }
        }
    }
}

#endif
