using System;

namespace TagBites.DB
{
    public interface IDbLinkContext : IDbLink
    {
        event EventHandler ConnectionClose;
        event DbLinkConnectionLostEventHandler ConnectionLost;
        event EventHandler ConnectionOpen;
        event DbExceptionFormatEventHandler ExceptionFormat;
        event DbLinkInfoMessageEventHandler InfoMessage;
        event DbLinkQueryEventHandler Query;
        event EventHandler<DbLinkQueryExecutedEventArgs> QueryExecuted;
        event EventHandler TransactionBeforeBegin;
        event EventHandler TransactionBegin;
        event EventHandler TransactionCommiting;
        event DbLinkTransactionCloseEventHandler TransactionClose;
        event EventHandler TransactionContextBegin;
        event DbLinkTransactionContextCloseEventHandler TransactionContextClose;

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
