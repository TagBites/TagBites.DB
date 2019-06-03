using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using TBS.Sql;

namespace TBS.Data.DB
{
    public abstract class DbLinkAdapter
    {
        public abstract SqlQueryResolver QueryResolver { get; }


        protected internal virtual DbLink CreateDbLink(DbLinkContext context)
        {
            return new DbLink(context);
        }
        protected internal virtual DbLinkContext CreateDbLinkContext(DbLinkProvider provider, Action<DbConnectionStringBuilder> connectionStringAdapter)
        {
            return new DbLinkContext(provider, connectionStringAdapter);
        }

        protected internal abstract DbConnection CreateConnection(string connectionString);
        protected internal abstract DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString);

        internal DbCommand CreateCommand(DbConnection connection, DbTransaction transaction, IQuerySource querySource)
        {
            var cmd = CreateCommandInner(QueryResolver.GetQuery(querySource));
            cmd.Connection = connection;
            cmd.Transaction = transaction;
            return cmd;
        }
        protected abstract DbCommand CreateCommandInner(Query query);

        protected internal abstract DbDataAdapter CreateDataAdapter(DbCommand command);
    }
}
