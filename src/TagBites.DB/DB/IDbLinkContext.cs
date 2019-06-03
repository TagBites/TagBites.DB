using System;
using TBS.Data.DB.Schema;

namespace TBS.Data.DB
{
    public interface IDbLinkContext : IDbLink
    {
        DbLinkBag Bag { get; }
        object SynchRoot { get; }
        string Database { get; set; }
        bool IsActive { get; }
        IDbLinkProvider Provider { get; }

        event EventHandler ConnectionClose;
        event DbLinkConnectionLostEventHandler ConnectionLost;
        event EventHandler ConnectionOpen;
        event DbExceptionFormatEventHandler ExceptionFormat;
        event DbLinkInfoMessageEventHandler InfoMessage;
        event DbLinkQueryEventHandler Query;
        event EventHandler TransactionBeforeBegin;
        event EventHandler TransactionBegin;
        event DbLinkTransactionCloseEventHandler TransactionClose;
        event EventHandler TransactionContextBegin;
        event DbLinkTransactionContextCloseEventHandler TransactionContextClose;

        IDbLink CreateLink();
    }
}
