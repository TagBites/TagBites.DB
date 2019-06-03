using System;
using System.Diagnostics;
using TBS.Resources;
using TBS.Utils;

namespace TBS.Data.DB
{
    public class DbLinkTransaction : IDbLinkTransaction
    {
        private DbLinkContext m_context;
        private bool m_executed;
        private readonly object m_locker;
        private readonly int m_nestingLevel;

        public IDbLinkTransactionContext Context => m_context.TransactionContext;

        internal DbLinkTransaction(DbLinkContext context)
        {
            Guard.ArgumentNotNull(context, "context");

            m_context = context;
            m_locker = m_context.SynchRoot;

            context.TransactionContextInternal.Attach();
            m_nestingLevel = context.TransactionContext.NestingLevel;
        }
        ~DbLinkTransaction()
        {
            Debug.WriteLine(ErrorMessages.UnexpectedFinalizerCalled(nameof(DbLinkTransaction)));
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
            lock (m_locker)
            {
                if (m_context == null)
                    throw new ObjectDisposedException("DbLinkTransactionWithScope");

                if (m_executed)
                    throw new InvalidOperationException("Commit/Rollback was already executed.");

                try
                {
                    m_context.MarkTransaction(rollback);
                }
                finally
                {
                    m_executed = true;
                }
            }
        }

        public void Dispose()
        {
            lock (m_locker)
            {
                if (m_context != null)
                {
                    try
                    {
                        if (!m_context.IsDisposed)
                            try
                            {
                                if (!m_executed)
                                    Rollback();
                            }
                            finally
                            {
                                m_context.CloseTransaction(m_nestingLevel);
                            }
                    }
                    finally
                    {
                        m_context = null;
                    }
                }
            }

            GC.SuppressFinalize(this);
        }
    }
}
