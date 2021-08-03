using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TagBites.DB.Postgres
{
    public sealed class PgSqlCursorManager : IDbCursorManager
    {
        internal readonly object PoolSynchRoot = new();

        private readonly List<PgSqlCursorConnectionContext> _connections = new List<PgSqlCursorConnectionContext>();

        public IDbLinkProvider LinkProvider { get; }
        public bool IsActive
        {
            get
            {
                lock (_connections)
                    return _connections.Any(x => x.IsActive);
            }
        }
        public int CursorCount
        {
            get
            {
                lock (_connections)
                    return _connections.Sum(x => x.CursorCount);
            }
        }
        public int ConnectionCount
        {
            get
            {
                lock (_connections)
                    return _connections.Sum(x => x.IsActive ? 1 : 0);
            }
        }

        /// <summary>
        /// Time in milliseconds to keep transaction alive. After that time transaction will be closed with all cursors.
        /// Cursor timeout will be between [<see cref="TransactionTimeout"/>/2, <see cref="TransactionTimeout"/>].
        /// After <see cref="TransactionTimeout"/>/2 all new cursors will be created on new transactions.
        /// Default 0 - no timeout. 
        /// </summary>
        public int TransactionTimeout { get; set; }

        /// <summary>
        /// Limit for active transactions.
        /// If <see cref="TransactionTimeout"/> is set and half of it time has passed then transaction is considered as inactive, cause it can not create new cursor and is marked for dispose.
        /// Therefore number of open transaction can be greater then <see cref="ActiveTransactionLimit"/>.
        /// <see cref="ActiveTransactionLimit"/> should be at least 2 times lower then link provider max pool size to avoid waiting for new connection.
        /// Default 0 - no limit. 
        /// </summary>
        public int ActiveTransactionLimit { get; set; }

        internal PgSqlCursorManager(PgSqlLinkProvider connectionProvider)
        {
            LinkProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        }
        ~PgSqlCursorManager()
        {
            Dispose(false);
        }


        public bool ContainsCursor(string cursorName) => GetCursor(cursorName) != null;
        public IDbCursor GetCursor(string cursorName)
        {
            if (string.IsNullOrEmpty(cursorName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(cursorName));

            lock (_connections)
                foreach (var connection in _connections)
                {
                    var cursor = connection.GetCursor(cursorName);
                    if (cursor != null)
                        return cursor;
                }

            return null;
        }

        public IDbCursor CreateCursor(IQuerySource querySource)
        {
            return CreateCursor(querySource, null, null);
        }
        public IDbCursor CreateCursor(IQuerySource querySource, string searchColumn, object searchId)
        {
            return CreateCursor(querySource, null, searchColumn, searchId, null, null);
        }
        public IDbCursor CreateCursor(IQuerySource querySource, IQuerySource queryCountSource, string searchColumn, object searchId, Action<IDbLink> beforeCreateAction, Action<IDbLink> cleanUpAction)
        {
            if (querySource == null)
                throw new ArgumentNullException(nameof(querySource));

            var timeout = TransactionTimeout;

            var maxConnections = ActiveTransactionLimit > 0 ? Math.Min(ActiveTransactionLimit, LinkProvider.MaxPoolSize) : LinkProvider.MaxPoolSize;
            if (timeout > 0 && maxConnections * 2 > LinkProvider.MaxPoolSize)
                maxConnections = Math.Max(1, maxConnections / 2);

            // Create context
            PgSqlCursorConnectionContext context = null;
            var isNew = false;

            lock (PoolSynchRoot)
            {
                // Existing context
                List<PgSqlCursorConnectionContext> connections;

                lock (_connections)
                    connections = _connections.ToList();

                if (connections.Count > 0)
                {
                    if (connections.Count > 1)
                        Shuffle(connections);

                    // First free
                    for (var i = connections.Count - 1; i >= 0; i--)
                    {
                        var connection = connections[i];

                        // Inactive or waiting for timeout
                        if (!connection.IsActive || timeout > 0 && connection.StartDateTime.AddMilliseconds(timeout / 2d) <= DateTime.Now)
                        {
                            connections.RemoveAt(i);
                            continue;
                        }

                        // Free
                        if (!connection.IsExecuting)
                        {
                            context = connection;
                            break;
                        }
                    }

                    // First active
                    if (context == null && maxConnections > 0 && connections.Count >= maxConnections)
                        context = connections.FirstOrDefault();
                }

                // Default context
                if (context == null)
                {
                    context = new PgSqlCursorConnectionContext(this);
                    isNew = true;

                    lock (_connections)
                        _connections.Add(context);
                }

                // Pending
                ++context.PendingCreateCursor;
            }

            // Execute
            lock (context.SynchRoot)
            {
                // Create connection outside of PoolSynchRoot to await deadlock when connection pool is exceeded in link provider
                context.TryCreateConnectionInternal();

                var cursor = context.CreateCursor(querySource, searchColumn, searchId, beforeCreateAction, cleanUpAction);

                if (timeout > 0 && isNew)
                {
                    Task.Run(async () =>
                    {
                        waiting:
                        await Task.Delay(timeout).ConfigureAwait(false);
                        timeout = Math.Min(timeout, Math.Max(timeout / 10, 1000));

                        lock (PoolSynchRoot)
                        {
                            while (context.PendingCreateCursor > 0)
                                goto waiting; // workaround for awaiting in lock

                            context.PendingDisposeInternal();
                        }

                        context.Dispose();
                    });
                }

                return cursor;
            }
        }

        public void Clear()
        {
            lock (_connections)
            {
                for (var i = _connections.Count - 1; i >= 0; i--)
                {
                    var connection = _connections[i];
                    _connections.RemoveAt(i);

                    connection.Dispose();
                }
            }
        }
        internal void OnConnectionContextDisposed(PgSqlCursorConnectionContext context)
        {
            lock (_connections)
                _connections.Remove(context);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            Clear();
        }

        private static void Shuffle<T>(IList<T> items)
        {
            var random = new Random();
            var n = items.Count;

            for (var i = 0; i < (n - 1); i++)
            {
                var r = i + random.Next(n - i);
                T t = items[r];
                items[r] = items[i];
                items[i] = t;
            }
        }
    }
}
