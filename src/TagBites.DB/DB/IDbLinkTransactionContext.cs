using System;

namespace TagBites.DB
{
    public interface IDbLinkTransactionContext
    {
        IDbLinkContext ConnectionContext { get; }
        DbLinkBag Bag { get; }
        Exception Exception { get; }
        int Level { get; }
        bool Started { get; }
        bool IsSystemTransaction { get; }
        DbLinkTransactionStatus Status { get; }

        event EventHandler TransactionBeforeBegin;
        event EventHandler TransactionBegin;
        event EventHandler TransactionBeforeCommit;
        event DbLinkTransactionCloseEventHandler TransactionClose;


        void Terminate();
    }
}
