using TagBites.DB;
using TagBites.DB.Postgres;
using TagBites.DB.SqlServer;
using TagBites.Sql;

namespace TagBites
{
    public partial class DbTests
    {
        private readonly object m_locker = new object();
        private PgSqlLinkProvider m_npgsqlProvider;

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

                    return m_npgsqlProvider;
                }
            }
        }
        public SqlServerLinkProvider SqlServerProvider { get; set; } = DbManager.CreateSqlServerProvider();

        public DbLinkProvider DefaultProvider => NpgsqlProvider;


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
