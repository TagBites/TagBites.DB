using System;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using TagBites.Utils;

namespace TagBites.DB
{
    /// <summary>
    /// Thread safe.
    /// </summary>
    public class DbLink : IDbLink
    {
        private DbLinkContext _context;
        private DbLinkContextSwitch _switcher;

        internal DbLinkContextSwitch Switcher
        {
            get => _switcher;
            set => _switcher = value;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsDisposed => _context == null;

        public DbLinkContext ConnectionContext
        {
            get
            {
                CheckDispose();
                return _context;
            }
            internal set
            {
                if (_context != null)
                    throw new InvalidOperationException();

                _context = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        public IDbLinkTransactionContext TransactionContext
        {
            get
            {
                CheckDispose();
                return _context.TransactionContext;
            }
        }
        public DbLinkTransactionStatus TransactionStatus
        {
            get
            {
                CheckDispose();
                return _context.TransactionStatus;
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
            _context.Force();
        }
        public IDbLinkTransaction Begin()
        {
            CheckDispose();
            return _context.Begin();
        }

        public int ExecuteNonQuery(IQuerySource query)
        {
            CheckDispose();
            return _context.ExecuteNonQuery(query);
        }

        public QueryResult Execute(IQuerySource query)
        {
            CheckDispose();
            return _context.Execute(query);
        }
        public object ExecuteScalar(IQuerySource query)
        {
            CheckDispose();
            return _context.ExecuteScalar(query);
        }

        public QueryResult[] BatchExecute(IQuerySource query)
        {
            CheckDispose();
            return _context.BatchExecute(query);
        }
        public DelayedBatchQueryResult DelayedBatchExecute(IQuerySource query)
        {
            CheckDispose();
            return _context.DelayedBatchExecute(query);
        }

        public T ExecuteOnAdapter<T>(IQuerySource query, Func<DbDataAdapter, T> executor)
        {
            CheckDispose();
            return _context.ExecuteOnAdapter(query, executor);
        }
        public T ExecuteOnReader<T>(IQuerySource query, Func<DbDataReader, T> executor)
        {
            CheckDispose();
            return _context.ExecuteOnReader(query, executor);
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
                if (Interlocked.Exchange(ref _context, null) is { } c)
                    c.Release();
            }
            finally
            {
                if (Interlocked.Exchange(ref _switcher, null) is { } s)
                    s.Dispose();
            }
        }
        protected virtual void Dispose(bool disposing)
        { }

        protected void CheckDispose()
        {
            if (_context == null)
                throw new ObjectDisposedException("DbLink");
        }
    }
}
