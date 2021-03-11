using System;
using System.Diagnostics;
using TagBites.Utils;

namespace TagBites.DB
{
    public class DbLinkTransaction : IDbLinkTransaction
    {
        private readonly DbLinkContext _context;
        private readonly object _locker;
        private readonly int _nestingLevel;
        private bool _executed;
        private bool _disposed;

        public IDbLinkContext ConnectionContext => _context;
        public IDbLinkTransactionContext Context => _context.TransactionContext;
        public DbLinkTransactionStatus Status => _context.TransactionContext.Status;

        internal DbLinkTransaction(DbLinkContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _locker = _context.SynchRoot;

            context.TransactionContextInternal.Attach();
            _nestingLevel = context.TransactionContext.Level;
        }
        ~DbLinkTransaction()
        {
            Debug.WriteLine(ErrorMessages.UnexpectedFinalizerCalled(nameof(DbLinkTransaction)));
            Dispose();
        }


        public void Commit() => CloseTransaction(false);
        public void Rollback() => CloseTransaction(true);
        private void CloseTransaction(bool rollback)
        {
            lock (_locker)
            {
                if (_disposed)
                    throw new ObjectDisposedException("DbLinkTransactionWithScope");

                if (_executed)
                    throw new InvalidOperationException("Commit/Rollback was already executed.");

                try
                {
                    _context.MarkTransaction(rollback);
                }
                finally
                {
                    _executed = true;
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            lock (_locker)
            {
                if (!_disposed)
                {
                    try
                    {
                        if (!_context.IsDisposed)
                            try
                            {
                                if (!_executed)
                                    Rollback();
                            }
                            finally
                            {
                                _context.CloseTransaction(_nestingLevel);
                            }
                    }
                    finally
                    {
                        _disposed = true;
                    }
                }
            }
        }
    }
}
