using System;
using System.Data.Common;

namespace TagBites.DB
{
    public class DbLinkTransactionContext : IDbLinkTransactionContext
    {
        private EventHandler m_transactionBeforeBegin;
        private EventHandler m_transactionBegin;
        private EventHandler m_transactionBeforeCommit;
        private DbLinkTransactionCloseEventHandler m_transactionClose;

        public event EventHandler TransactionBeforeBegin
        {
            add
            {
                lock (m_locker)
                {
                    CheckDispose();
                    m_transactionBeforeBegin += value;
                    m_context.TransactionBeforeBegin += OnTransactionBeforeBegin;
                }
            }
            remove
            {
                lock (m_locker)
                {
                    CheckDispose();
                    m_transactionBeforeBegin -= value;
                    m_context.TransactionBeforeBegin -= OnTransactionBeforeBegin;
                }
            }
        }
        public event EventHandler TransactionBegin
        {
            add
            {
                lock (m_locker)
                {
                    CheckDispose();
                    m_transactionBegin += value;
                    m_context.TransactionBegin += OnTransactionBegin;
                }
            }
            remove
            {
                lock (m_locker)
                {
                    CheckDispose();
                    m_transactionBegin -= value;
                    m_context.TransactionBegin -= OnTransactionBegin;
                }
            }
        }
        public event EventHandler TransactionBeforeCommit
        {
            add
            {
                lock (m_locker)
                {
                    CheckDispose();
                    m_transactionBeforeCommit += value;
                    m_context.TransactionBeforeCommit += OnTransactionBeforeCommit;
                }
            }
            remove
            {
                lock (m_locker)
                {
                    CheckDispose();
                    m_transactionBeforeCommit -= value;
                    m_context.TransactionBeforeCommit -= OnTransactionBeforeCommit;
                }
            }
        }
        public event DbLinkTransactionCloseEventHandler TransactionClose
        {
            add
            {
                lock (m_locker)
                {
                    CheckDispose();
                    m_transactionClose += value;
                    m_context.TransactionClose += OnTransactionClose;
                }
            }
            remove
            {
                lock (m_locker)
                {
                    CheckDispose();
                    m_transactionClose -= value;
                    m_context.TransactionClose -= OnTransactionClose;
                }
            }
        }

        private readonly object m_locker;
        private DbLinkContext m_context;
        private DbLinkBag m_bag;

        public DbLinkContext ConnectionContext
        {
            get
            {
                lock (m_locker)
                {
                    CheckDispose();
                    return m_context;
                }
            }
        }
        IDbLinkContext IDbLinkTransactionContext.ConnectionContext => ConnectionContext;
        public DbLinkBag Bag
        {
            get
            {
                lock (m_locker)
                {
                    if (m_bag == null)
                        m_bag = new DbLinkBag(m_locker);

                    return m_bag;
                }
            }
        }

        public bool Started { get; internal set; }
        public Exception Exception { get; internal set; }
        public int Level => TransactionRefferenceCountInternal;
        public bool IsSystemTransaction { get; internal set; }
        public DbLinkTransactionStatus Status { get; internal set; }

        internal System.Transactions.Transaction SystemTransactionInternal { get; set; }
        internal DbTransaction DbTransactionInternal { get; set; }
        internal int TransactionRefferenceCountInternal { get; private set; }

        internal DbLinkTransactionContext(DbLinkContext context, DbLinkTransactionStatus status, bool isSystemTransaction)
        {
            m_context = context;
            m_locker = context.SynchRoot;
            Status = status;
            IsSystemTransaction = isSystemTransaction;
        }


        public void Terminate()
        {
            m_context.MarkTransaction(true);
        }

        internal void Attach()
        {
            lock (m_locker)
            {
                CheckDispose();
                ++TransactionRefferenceCountInternal;
            }
        }
        internal bool BeginRelease()
        {
            lock (m_locker)
            {
                if (m_context == null) // Disposed
                    return false;

                return --TransactionRefferenceCountInternal == 0;
            }
        }
        internal void ForceRelease()
        {
            lock (m_locker)
            {
                if (m_context == null) // Disposed
                    return;

                if (m_transactionBeforeBegin != null)
                    m_context.TransactionBeforeBegin -= m_transactionBeforeBegin;

                if (m_transactionBegin != null)
                    m_context.TransactionBegin -= OnTransactionBegin;

                if (m_transactionBeforeCommit != null)
                    m_context.TransactionBeforeCommit -= OnTransactionBeforeCommit;

                if (m_transactionClose != null)
                    m_context.TransactionClose -= OnTransactionClose;

                m_context = null;
            }
        }

        protected void CheckDispose()
        {
            if (m_context == null)
                throw new ObjectDisposedException("DbLinkTransactionContext");
        }

        private void OnTransactionBeforeBegin(object sender, EventArgs e)
        {
            lock (m_locker)
            {
                if (m_transactionBeforeBegin != null)
                    m_transactionBeforeBegin(this, e);
            }
        }
        private void OnTransactionBegin(object sender, EventArgs e)
        {
            lock (m_locker)
            {
                if (m_transactionBegin != null)
                    m_transactionBegin(this, e);
            }
        }
        private void OnTransactionBeforeCommit(object sender, EventArgs e)
        {
            lock (m_locker)
            {
                if (m_transactionBeforeCommit != null)
                    m_transactionBeforeCommit(this, e);
            }
        }
        private void OnTransactionClose(object sender, DbLinkTransactionCloseEventArgs e)
        {
            lock (m_locker)
            {
                if (m_transactionClose != null)
                    m_transactionClose(this, e);
            }
        }
    }
}
