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

        event EventHandler TransactionBeginning;
        event EventHandler TransactionBegan;
        event EventHandler TransactionCommiting;
        event EventHandler<DbLinkTransactionCloseEventArgs> TransactionClosed;
        event EventHandler<DbLinkTransactionContextCloseEventArgs> TransactionContextClosed;


        void Terminate();
    }
}
