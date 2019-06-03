using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using TBS.Data.DB;
using TBS.Data.DB.PostgreSql;
using TBS.Data.DB.SQLite;
using TBS.Sql;

namespace TBS.Data.UnitTests.DB
{
	public class DbTestBase
	{
		private readonly object m_locker = new object();
		private PgSqlLinkProvider m_npgsqlProvider;
		private SQLiteLinkProvider m_sqLiteProvider;

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
		public SQLiteLinkProvider SQLiteProvider
		{
			get
			{
				lock (m_locker)
				{
					if (m_sqLiteProvider == null)
						m_sqLiteProvider = DbManager.CreateSQLiteProvider();

					SqlQueryResolver.DefaultToStringResolver = m_sqLiteProvider.QueryResolver;
					return m_sqLiteProvider;
				}
			}
		}
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
