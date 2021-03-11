using System;

namespace TagBites.DB
{
    public interface IDbLinkContext : IDbLink
    {
        event EventHandler ConnectionClosed;
        event EventHandler<DbLinkConnectionLostEventArgs> ConnectionLost;
        event EventHandler ConnectionOpened;
        event EventHandler<DbExceptionFormatEventArgs> ExceptionFormatting;
        event EventHandler<DbLinkInfoMessageEventArgs> InfoMessageReceived;
        event EventHandler<DbLinkQueryExecutingEventArgs> QueryExecuting;
        event EventHandler<DbLinkQueryExecutedEventArgs> QueryExecuted;
        event EventHandler TransactionBeginning;
        event EventHandler TransactionBegan;
        event EventHandler TransactionCommiting;
        event EventHandler<DbLinkTransactionCloseEventArgs> TransactionClosed;
        event EventHandler TransactionContextBegan;
        event EventHandler<DbLinkTransactionContextCloseEventArgs> TransactionContextClosed;

        IDbLinkProvider Provider { get; }
        DbLinkBag Bag { get; }
        object SynchRoot { get; }
        bool IsActive { get; }
        bool IsExecuting { get; }
        string Database { get; set; }
        DateTime LastExecuted { get; }



        IDbLink CreateLink();
    }
}
