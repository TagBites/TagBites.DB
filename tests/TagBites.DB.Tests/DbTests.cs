using TagBites.DB;
using TagBites.DB.Postgres;
using TagBites.DB.SqLite;
using TagBites.Sql;
using Xunit; //using TagBites.DB.SqLite;

namespace TagBites
{
    public class DbTests
    {
        private readonly object m_locker = new object();
        private PgSqlLinkProvider m_npgsqlProvider;
        private SqliteLinkProvider _mSqliteProvider;

        public PgSqlLinkProvider NpgsqlProvider
        {
            get
            {
                lock (m_locker)
                {
                    if (m_npgsqlProvider == null)
                    {
                        m_npgsqlProvider = DbManager.CreateNpgsqlProvider();
                        InitializeConnectionProvider(m_npgsqlProvider);
                    }

                    SqlQueryResolver.DefaultToStringResolver = m_npgsqlProvider.QueryResolver;
                    return m_npgsqlProvider;
                }
            }
        }
        public SqliteLinkProvider SqliteProvider
        {
            get
            {
                lock (m_locker)
                {
                    if (_mSqliteProvider == null)
                        _mSqliteProvider = DbManager.CreateSQLiteProvider();

                    SqlQueryResolver.DefaultToStringResolver = _mSqliteProvider.QueryResolver;
                    return _mSqliteProvider;
                }
            }
        }
        public DbLinkProvider DefaultProvider => NpgsqlProvider;

        public DbTests()
        {
            Assert.Null(DefaultProvider.CurrentConnectionContext);
        }


        protected virtual void InitializeConnectionProvider(PgSqlLinkProvider connectionProvider) { }

        protected virtual IDbLink CreateLink()
        {
            return DefaultProvider.CreateLink();
        }
        protected virtual T ExecuteScalar<T>(Query query)
        {
            using (var link = CreateLink())
                return link.ExecuteScalar<T>(query);
        }
        protected virtual T ExecuteScalar<T>(SqlExpression expression)
        {
            var q = new SqlQuerySelect();
            q.Select.Add(expression);

            using (var link = CreateLink())
                return link.ExecuteScalar<T>(q);
        }
    }
}
