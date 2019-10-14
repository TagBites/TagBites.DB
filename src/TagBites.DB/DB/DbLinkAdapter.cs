using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using TagBites.Sql;

namespace TagBites.DB
{
    public abstract class DbLinkAdapter
    {
        public abstract SqlQueryResolver QueryResolver { get; }
        public abstract int DefaultPort { get; }


        protected internal virtual DbLink CreateDbLink()
        {
            return new DbLink();
        }
        protected internal virtual DbLinkContext CreateDbLinkContext()
        {
            return new DbLinkContext();
        }

        protected internal abstract DbConnection CreateConnection(string connectionString);
        protected internal abstract string CreateConnectionString(DbConnectionArguments connectionArguments);

        internal DbCommand CreateCommand(DbConnection connection, DbTransaction transaction, IQuerySource querySource)
        {
            var cmd = CreateCommand(QueryResolver.GetQuery(querySource));
            cmd.Connection = connection;
            cmd.Transaction = transaction;
            return cmd;
        }
        protected abstract DbCommand CreateCommand(Query query);

        protected internal abstract DbDataAdapter CreateDataAdapter(DbCommand command);
    }
}
