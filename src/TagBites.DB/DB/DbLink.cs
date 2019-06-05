using System;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using TBS.Resources;

namespace TBS.Data.DB
{
    /// <summary>
    /// Thread safe.
    /// </summary>
    public class DbLink : IDbLink
    {
        private DbLinkContext m_context;

        internal DbLinkContextSwitch Switcher { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsDisposed => m_context == null;

        public DbLinkContext ConnectionContext
        {
            get
            {
                CheckDispose();
                return m_context;
            }
            internal set
            {
                if (m_context != null)
                    throw new InvalidOperationException();

                m_context = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        public IDbLinkTransactionContext TransactionContext
        {
            get
            {
                CheckDispose();
                return m_context.TransactionContext;
            }
        }
        public DbLinkTransactionStatus TransactionStatus
        {
            get
            {
                CheckDispose();
                return m_context.TransactionStatus;
            }
        }

        IDbLinkContext IDbLink.ConnectionContext => ConnectionContext;

        protected internal DbLink()
        { }
        ~DbLink()
        {
            Debug.WriteLine(ErrorMessages.UnexpectedFinalizerCalled(nameof(DbLink)));
            DisposeCore();
            Dispose(false);
        }


        public void Force()
        {
            CheckDispose();
            m_context.Force();
        }
        public IDbLinkTransaction Begin()
        {
            CheckDispose();
            return m_context.Begin();
        }

        public int ExecuteNonQuery(IQuerySource query)
        {
            CheckDispose();
            return m_context.ExecuteNonQuery(query);
        }

        public QueryResult Execute(IQuerySource query)
        {
            CheckDispose();
            return m_context.Execute(query);
        }
        public object ExecuteScalar(IQuerySource query)
        {
            CheckDispose();
            return m_context.ExecuteScalar(query);
        }

        public QueryResult[] BatchExecute(IQuerySource query)
        {
            CheckDispose();
            return m_context.BatchExecute(query);
        }
        public DelayedBatchQueryResult DelayedBatchExecute(IQuerySource query)
        {
            CheckDispose();
            return m_context.DelayedBatchExecute(query);
        }

        public T ExecuteOnAdapter<T>(IQuerySource query, Func<DbDataAdapter, T> executor)
        {
            CheckDispose();
            return m_context.ExecuteOnAdapter(query, executor);
        }
        public T ExecuteOnReader<T>(IQuerySource query, Func<DbDataReader, T> executor)
        {
            CheckDispose();
            return m_context.ExecuteOnReader(query, executor);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DisposeCore();
            Dispose(true);
        }
        private void DisposeCore()
        {
            try
            {
                if (m_context != null)
                    try
                    {
                        m_context.Release();
                    }
                    finally
                    {
                        m_context = null;
                    }
            }
            finally
            {
                if (Switcher != null)
                    try
                    {
                        Switcher.Dispose();
                    }
                    finally
                    {
                        Switcher = null;
                    }
            }
        }
        protected virtual void Dispose(bool disposing)
        { }

        protected void CheckDispose()
        {
            if (m_context == null)
                throw new ObjectDisposedException("DbLink");
        }
    }
}
