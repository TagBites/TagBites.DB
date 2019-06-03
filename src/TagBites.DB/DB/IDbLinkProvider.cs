using TBS.Data.DB.Configuration;
using TBS.Sql;

namespace TBS.Data.DB
{
    public interface IDbLinkProvider
    {
        event DbLinkContextEventHandler ContextCreated;

        int ActiveConnectionsCount { get; }
        DbLinkConfiguration Configuration { get; }
        int ConnectionsCount { get; }
        string Database { get; }
        bool IsCursorSupported { get; }
        int MaxPoolSize { get; }
        int MinPoolSize { get; }
        int PoolConnectionsCount { get; }
        int Port { get; }
        SqlQueryResolver QueryResolver { get; }
        string Server { get; }
        bool UsePooling { get; }
        int UsingConnectionsCount { get; }


        IDbCursorManager CreateCursorManager();

        IDbLink CreateExclusiveLink();
        IDbLink CreateLink();
        IDbLink CreateLink(DbLinkCreateOption createOption);
    }
}
