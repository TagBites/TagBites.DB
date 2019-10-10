using System;
using System.Collections.Generic;
using System.Threading;

namespace TBS.Data.DB.PostgreSql
{
    internal class PgSqlCursorConnectionContext : IDbCursorOwner
    {
        private static int s_nextCursorIndex;

        internal readonly object SynchRoot = new object();

        private PgSqlCursorManager _manager;
        private IDbLink _link;
        private IDbLinkTransaction _transaction;
        private readonly List<PgSqlCursor> _cursors = new List<PgSqlCursor>();

        IDbLinkProvider IDbCursorOwner.LinkProvider => _manager.LinkProvider;
        public PgSqlCursorManager Manager => _manager;
        public bool IsActive => _link?.TransactionStatus == DbLinkTransactionStatus.Open;
        public int CursorCount => IsActive ? _cursors.Count : 0;
        public DateTime StartDateTime { get; } = DateTime.Now;

        public PgSqlCursorConnectionContext(PgSqlCursorManager manager)
        {
            _manager = manager;
            _link = manager.LinkProvider.CreateLink(DbLinkCreateOption.RequiresNew);
            _transaction = ((PgSqlLinkContext)_link.ConnectionContext).BeginForCursorManager();
        }
        ~PgSqlCursorConnectionContext()
        {
            Dispose(false);
        }


        public bool ContainsCursor(string cursorName) => GetCursor(cursorName) != null;
        public PgSqlCursor GetCursor(string cursorName)
        {
            if (string.IsNullOrEmpty(cursorName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(cursorName));

            cursorName = cursorName.ToLower();

            lock (SynchRoot)
            {
                if (!IsActive)
                    return null;

                foreach (var c in _cursors)
                    if (c.Name == cursorName)
                        return c;
            }

            return null;
        }

        public PgSqlCursor CreateCursor(IQuerySource querySource, string searchColumn, object searchId, Action<IDbLink> beforeCreateAction, Action<IDbLink> cleanUpAction)
        {
            lock (SynchRoot)
            {
                ThrowIfNotActive();

                try
                {
                    // Prepare action
                    beforeCreateAction?.Invoke(_link);

                    // Create cursor
                    var queryResolver = _link.ConnectionContext.Provider.QueryResolver;
                    var query = queryResolver.GetQuery(querySource);
                    var queryString = query.GetUnsafeEscapeString(queryResolver);

                    var cursorName = String.Format("cs_cursor_{0}", Interlocked.Increment(ref s_nextCursorIndex));
                    var cursorQuery = new Query("SELECT CreateCursorC({0}, {1}, {2}, {3})", cursorName, queryString, searchColumn ?? string.Empty, (searchId ?? string.Empty).ToString());
                    var result = _link.ExecuteScalar<string>(cursorQuery);

                    var values = result?.Split('|');
                    if (values?.Length != 2)
                        throw new Exception("Function CreateCursorC returns invalid result.");

                    var rowCount = int.Parse(values[0]);
                    var searchIndex = int.Parse(values[1]);

                    var cursor = new PgSqlCursor(
                        this,
                        query,
                        cursorName,
                        rowCount,
                        searchIndex,
                        cleanUpAction);

                    _cursors.Add(cursor);
                    return cursor;
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }
        }
        public QueryResult FetchCursor(PgSqlCursor cursor, int index, int count)
        {
            if (index < 0 || count < 0 || (index + count) > cursor.RecordCount)
                throw new IndexOutOfRangeException();

            if (cursor.RecordCount == 0 || count == 0)
                return QueryResult.Empty;

            lock (SynchRoot)
            {
                ThrowIfNotActive();

                var ci = cursor.Position;
                int shift;
                string direction;

                if (index > ci)
                {
                    direction = "FORWARD";
                    shift = index - ci;
                    ci += shift;
                }
                else
                {
                    direction = "BACKWARD";
                    shift = ci - index;
                    ci -= shift;
                }

                // Result
                try
                {
                    if (shift > 0)
                    {
                        _link.ExecuteNonQuery($"MOVE {direction} {shift} IN {cursor.Name}");
                        cursor.Position = ci;
                    }

                    var result = _link.Execute($"FETCH {count} FROM {cursor.Name}");

                    cursor.Position += result.RowCount;
                    return result;
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }
        }
        public void CloseCursor(PgSqlCursor cursor)
        {
            lock (SynchRoot)
            {
                if (!_cursors.Remove(cursor))
                    return;

                ThrowIfNotActive();

                try
                {
                    _link.ExecuteNonQuery($"CLOSE {cursor.Name}");
                    cursor.CleanUpAction?.Invoke(_link);
                }
                catch
                {
                    Dispose();
                    throw;
                }

                if (_cursors.Count == 0)
                    Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            try { _manager?.OnConnectionContextDisposed(this); }
            catch { /* ignored */ }
            finally { _manager = null; }

            lock (SynchRoot)
            {
                _cursors.Clear();

                if (_transaction != null)
                    try { _transaction.Dispose(); }
                    catch { /* ignored */ }
                    finally { _transaction = null; }

                if (_link != null)
                    try { _link.Dispose(); }
                    catch { /* ignored */ }
                    finally { _link = null; }
            }
        }
        private void ThrowIfNotActive()
        {
            if (!IsActive)
                throw new ObjectDisposedException("Cursor connection is closed.");
        }
    }
}
