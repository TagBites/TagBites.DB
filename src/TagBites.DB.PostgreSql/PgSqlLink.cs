namespace TBS.Data.DB.PostgreSql
{
    public class PgSqlLink : DbLink
    {
        public new PgSqlLinkContext ConnectionContext => (PgSqlLinkContext)base.ConnectionContext;

        protected internal PgSqlLink()
        { }


        public void Notify(string channel, string message) => ConnectionContext.Notify(channel, message);
        public void Listen(params string[] channels) => ConnectionContext.Listen(channels);
        public void Unlisten(params string[] channels) => ConnectionContext.Unlisten(channels);
        public void UnlistenAll() => ConnectionContext.UnlistenAll();
    }
}
