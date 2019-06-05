namespace TBS.Data.DB.PostgreSql
{
    public abstract class PgSqlLinkProvider : DbLinkProvider
    {
        protected PgSqlLinkProvider(DbLinkAdapter adapter, string connectionString)
            : this(adapter, new DbConnectionArguments(connectionString))
        { }
        protected PgSqlLinkProvider(DbLinkAdapter adapter, DbConnectionArguments arguments)
            : base(adapter, arguments)
        { }


        public PgSqlCursorManager CreateCursorManager() => (PgSqlCursorManager)CreateCursorManagerInner();
        protected override IDbCursorManager CreateCursorManagerInner() => new PgSqlCursorManager(this);

        public new PgSqlLink CreateLink() => (PgSqlLink)base.CreateLink();
        public new PgSqlLink CreateLink(DbLinkCreateOption createOption) => (PgSqlLink)base.CreateLink(createOption);

        public new PgSqlLink CreateExclusiveLink() => (PgSqlLink)base.CreateExclusiveLink();

        protected internal abstract PgSqlLink CreateExclusiveNotifyLink();
    }
}
