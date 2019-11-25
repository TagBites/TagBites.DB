using System;
using System.Diagnostics;
using System.Transactions;
using TagBites.Utils;

namespace TagBites.DB
{
    internal class DbLinkTransactionWithScope : IDbLinkTransaction
    {
        private DbLinkContext m_context;
        private Transaction m_transaction;
        private TransactionScope m_transactionScope;
        private bool m_executed;
        private readonly int m_nestingLevel;
        private readonly object m_synchRoot;

        public IDbLinkContext ConnectionContext => m_context;
        public IDbLinkTransactionContext Context => m_context.TransactionContext;

        internal DbLinkTransactionWithScope(DbLinkContext context, TransactionScope transactionScope, Transaction transaction)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            Guard.ArgumentNotNull(transactionScope, nameof(transactionScope));
            Guard.ArgumentNotNull(transaction, nameof(transaction));

            m_context = context;
            m_transactionScope = transactionScope;
            m_transaction = transaction;
            m_synchRoot = context.SynchRoot;

            context.TransactionContextInternal.Attach();
            m_nestingLevel = context.TransactionContext.Level;
        }
        ~DbLinkTransactionWithScope()
        {
            Debug.WriteLine(ErrorMessages.UnexpectedFinalizerCalled(nameof(DbLinkTransactionWithScope)));
            Dispose();
        }


        public void Commit()
        {
            CloseTransaction(false);
        }
        public void Rollback()
        {
            CloseTransaction(true);
        }
        private void CloseTransaction(bool rollback)
        {
            if (m_context == null)
                throw new ObjectDisposedException("DbLinkTransactionWithScope");

            if (m_executed)
                throw new InvalidOperationException("Commit/Rollback was already executed.");

            if (m_context.TransactionStatus == DbLinkTransactionStatus.RollingBack && !rollback)
                throw new InvalidOperationException("Can not commit already rollback transaction.");

            try
            {
                if (rollback)
                    m_transaction.Rollback();
                else
                    m_transactionScope.Complete();
            }
            finally
            {
                m_executed = true;
            }
        }

        public void Dispose()
        {
            lock (m_synchRoot)
            {
                if (m_transactionScope != null)
                    try
                    {
                        m_context.CloseTransaction(m_nestingLevel);
                    }
                    finally
                    {
                        try
                        {
                            m_transactionScope.Dispose();
                        }
                        finally
                        {
                            m_context = null;
                            m_transaction = null;
                            m_transactionScope = null;
                        }
                    }
            }

            GC.SuppressFinalize(this);
        }
    }
}
