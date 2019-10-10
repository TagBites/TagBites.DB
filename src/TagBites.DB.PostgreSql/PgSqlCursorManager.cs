using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TBS.Data.DB.PostgreSql
{
    public sealed class PgSqlCursorManager : IDbCursorManager
    {
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
            List<PgSqlCursorConnectionContext> connections;

            lock (_connections)
                connections = _connections.ToList();

            foreach (var connection in connections)
            {
                lock (connection.SynchRoot)
                {
                    if (!connection.IsActive)
                        continue;

                    // ReSharper disable once PossibleLossOfFraction
                    if (timeout == 0 || connection.StartDateTime.AddMilliseconds(timeout / 2) > DateTime.Now)
                    {
                        return connection.CreateCursor(querySource, searchColumn, searchId, beforeCreateAction, cleanUpAction);
                    }
                }
            }

            var context = new PgSqlCursorConnectionContext(this);

            lock (_connections)
                _connections.Add(context);

            var cursor = context.CreateCursor(querySource, searchColumn, searchId, beforeCreateAction, cleanUpAction);

            if (timeout > 0)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(timeout).ConfigureAwait(false);
                    context.Dispose();
                });
            }

            return cursor;
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
    }
}
