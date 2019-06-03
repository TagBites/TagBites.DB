using System;
using System.Data.Common;

namespace TBS.Data.DB.PostgreSql
{
    public abstract class PgSqlLinkProvider : DbLinkProvider
    {
        protected PgSqlLinkProvider(DbLinkAdapter adapter, string connectionString)
            : base(adapter, connectionString)
        { }


        public PgSqlCursorManager CreateCursorManager()
        {
            return (PgSqlCursorManager)CreateCursorManagerInner();
        }
        protected override IDbCursorManager CreateCursorManagerInner()
        {
            return new PgSqlCursorManager(this);
        }

        public new PgSqlLink CreateLink()
        {
            return (PgSqlLink)base.CreateLink();
        }
        public new PgSqlLink CreateLink(DbLinkCreateOption createOption)
        {
            return (PgSqlLink)base.CreateLink(createOption);
        }

        public new PgSqlLink CreateExclusiveLink()
        {
            return (PgSqlLink)base.CreateExclusiveLink();
        }
        public new PgSqlLink CreateExclusiveLink(Action<DbConnectionStringBuilder> connectionStringAdapter)
        {
            return (PgSqlLink)base.CreateExclusiveLink(connectionStringAdapter);
        }

        protected internal abstract PgSqlLink CreateExclusiveNotifyLink();
    }
}
