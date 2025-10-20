using System;
using System.Data.Common;

namespace TagBites.DB
{
    public class DbLinkTransactionContext : IDbLinkTransactionContext
    {
        private readonly object _locker;
        private readonly DbLinkContext _context;
        private DbLinkBag _bag;
        private bool _disposed;

        private EventHandler _transactionBeginning;
        private EventHandler _transactionBegan;
        private EventHandler _transactionCommiting;
        private EventHandler<DbLinkTransactionCloseEventArgs> _transactionClosed;
        private EventHandler<DbLinkTransactionContextCloseEventArgs> _transactionContextClosed;

        public event EventHandler TransactionBeginning
        {
            add
            {
                lock (_locker)
                {
                    var attached = _transactionBeginning != null;
                    _transactionBeginning += value;

                    if (!attached && _transactionBeginning != null && !_disposed)
                        _context.TransactionBeginning += OnTransactionBeginning;
                }
            }
            remove
            {
                lock (_locker)
                {
                    var attached = _transactionBeginning != null;
                    _transactionBeginning -= value;

                    if (attached && _transactionBeginning == null && !_disposed)
                        _context.TransactionBeginning -= OnTransactionBeginning;
                }
            }
        }
        public event EventHandler TransactionBegan
        {
            add
            {
                lock (_locker)
                {
                    var attached = _transactionBegan != null;
                    _transactionBegan += value;

                    if (!attached && _transactionBegan != null && !_disposed)
                        _context.TransactionBegan += OnTransactionBegan;
                }
            }
            remove
            {
                lock (_locker)
                {
                    var attached = _transactionBegan != null;
                    _transactionBegan -= value;

                    if (attached && _transactionBegan == null && !_disposed)
                        _context.TransactionBegan -= OnTransactionBegan;
                }
            }
        }
        public event EventHandler TransactionCommiting
        {
            add
            {
                lock (_locker)
                {
                    var attached = _transactionCommiting != null;
                    _transactionCommiting += value;

                    if (!attached && _transactionCommiting != null && !_disposed)
                        _context.TransactionCommiting += OnTransactionCommiting;
                }
            }
            remove
            {
                lock (_locker)
                {
                    var attached = _transactionCommiting != null;
                    _transactionCommiting -= value;

                    if (attached && _transactionCommiting == null && !_disposed)
                        _context.TransactionCommiting -= OnTransactionCommiting;
                }
            }
        }
        public event EventHandler<DbLinkTransactionCloseEventArgs> TransactionClosed
        {
            add
            {
                lock (_locker)
                {
                    var attached = _transactionClosed != null;
                    _transactionClosed += value;

                    if (!attached && _transactionClosed != null && !_disposed)
                        _context.TransactionClosed += OnTransactionClosed;
                }
            }
            remove
            {
                lock (_locker)
                {
                    var attached = _transactionClosed != null;
                    _transactionClosed -= value;

                    if (attached && _transactionClosed == null && !_disposed)
                        _context.TransactionClosed -= OnTransactionClosed;
                }
            }
        }
        public event EventHandler<DbLinkTransactionContextCloseEventArgs> TransactionContextClosed
        {
            add
            {
                lock (_locker)
                {
                    var attached = _transactionContextClosed != null;
                    _transactionContextClosed += value;

                    if (!attached && _transactionContextClosed != null && !_disposed)
                        _context.TransactionContextClosed += OnTransactionContextClosed;
                }
            }
            remove
            {
                lock (_locker)
                {
                    var attached = _transactionContextClosed != null;
                    _transactionContextClosed -= value;

                    if (attached && _transactionContextClosed == null && !_disposed)
                        _context.TransactionContextClosed -= OnTransactionContextClosed;
                }
            }
        }

        public DbLinkContext ConnectionContext => _context;
        IDbLinkContext IDbLinkTransactionContext.ConnectionContext => ConnectionContext;
        public DbLinkBag Bag => _bag ??= new DbLinkBag(_locker);

        public bool Started { get; internal set; }
        public Exception Exception { get; internal set; }
        public int Level => TransactionReferenceCountInternal;
        public bool IsSystemTransaction { get; internal set; }
        public DbLinkTransactionStatus Status { get; internal set; }

        internal System.Transactions.Transaction SystemTransactionInternal { get; set; }
        internal DbTransaction DbTransactionInternal { get; set; }
        internal int TransactionReferenceCountInternal { get; private set; }

        internal DbLinkTransactionContext(DbLinkContext context, DbLinkTransactionStatus status, bool isSystemTransaction)
        {
            _context = context;
            _locker = context.SynchRoot;
            Status = status;
            IsSystemTransaction = isSystemTransaction;
        }


        public void Terminate() => _context.MarkTransaction(true);

        internal void Attach()
        {
            lock (_locker)
            {
                CheckDispose();
                ++TransactionReferenceCountInternal;
            }
        }
        internal bool BeginRelease()
        {
            lock (_locker)
            {
                if (_disposed)
                    return false;

                return --TransactionReferenceCountInternal == 0;
            }
        }
        internal void ForceRelease()
        {
            lock (_locker)
            {
                if (_disposed)
                    return;

                if (_transactionBeginning != null)
                    _context.TransactionBeginning -= OnTransactionBeginning;

                if (_transactionBegan != null)
                    _context.TransactionBegan -= OnTransactionBegan;

                if (_transactionCommiting != null)
                    _context.TransactionCommiting -= OnTransactionCommiting;

                if (_transactionClosed != null)
                    _context.TransactionClosed -= OnTransactionClosed;

                if (_transactionContextClosed != null)
                    _context.TransactionContextClosed -= OnTransactionContextClosed;

                _disposed = true;
            }
        }

        protected void CheckDispose()
        {
            if (_disposed)
                throw new ObjectDisposedException("DbLinkTransactionContext");
        }

        private void OnTransactionBeginning(object sender, EventArgs e) => _transactionBeginning?.Invoke(this, e);
        private void OnTransactionBegan(object sender, EventArgs e) => _transactionBegan?.Invoke(this, e);
        private void OnTransactionCommiting(object sender, EventArgs e)
        {
            if (_transactionCommiting?.GetInvocationList() is { } events)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < events.Length; i++)
                {
                    var action = (EventHandler)events[i];
                    action(this, EventArgs.Empty);

                    if (Status == DbLinkTransactionStatus.RollingBack)
                        break;
                }
            }
        }
        private void OnTransactionClosed(object sender, DbLinkTransactionCloseEventArgs e) => _transactionClosed?.Invoke(this, e);
        private void OnTransactionContextClosed(object sender, DbLinkTransactionContextCloseEventArgs e) => _transactionContextClosed?.Invoke(this, e);
    }
}
