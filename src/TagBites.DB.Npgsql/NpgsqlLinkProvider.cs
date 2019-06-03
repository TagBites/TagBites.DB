using Npgsql;

namespace TBS.Data.DB.PostgreSql
{
    public class NpgsqlLinkProvider : PgSqlLinkProvider
    {
        public NpgsqlLinkProvider(string connectionString)
            : base(new NpgsqlLinkAdapter(), connectionString)
        { }


        protected override PgSqlLink CreateExclusiveNotifyLink()
        {
            return CreateExclusiveLink(x =>
            {
                var cs = (NpgsqlConnectionStringBuilder)x;
                cs.KeepAlive = 0;
                cs.TcpKeepAlive = true;
                cs.TcpKeepAliveTime = 1;
                cs.TcpKeepAliveInterval = 50;
                cs.Pooling = false;
            });
        }
    }
}
