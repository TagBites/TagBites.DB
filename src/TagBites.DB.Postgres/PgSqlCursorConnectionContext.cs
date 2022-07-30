using System;
using System.Collections.Generic;
using System.Threading;

namespace TagBites.DB.Postgres
{
    internal class PgSqlCursorConnectionContext : IDbCursorOwner
    {
        private static int s_nextCursorIndex;

        private readonly object _synchRoot = new();
        private IDbLink _link;
        private IDbLinkTransaction _transaction;
        private readonly List<PgSqlCursor> _cursors = new();
        private bool _disposed;

        internal int PendingCreateCursor { get; set; }

        IDbLinkProvider IDbCursorOwner.LinkProvider => Manager.LinkProvider;
        public PgSqlCursorManager Manager { get; private set; }
        public bool IsNew { get; private set; } = true;
        public bool IsActive { get; private set; }
        public bool IsExecuting => IsActive && _link?.ConnectionContext?.IsExecuting == true;
        public int CursorCount
        {
            get
            {
                if (IsActive)
                    lock (_cursors)
                        return _cursors.Count;

                return 0;
            }
        }
        public DateTime StartDateTime { get; } = DateTime.Now;

        public PgSqlCursorConnectionContext(PgSqlCursorManager manager) => Manager = manager;
        ~PgSqlCursorConnectionContext() => Dispose(false);


        public bool ContainsCursor(string cursorName) => GetCursor(cursorName) != null;
        public PgSqlCursor GetCursor(string cursorName)
        {
            if (string.IsNullOrEmpty(cursorName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(cursorName));

            cursorName = cursorName.ToLower();

            if (!IsActive)
                return null;

            lock (_cursors)
                foreach (var c in _cursors)
                    if (c.Name == cursorName)
                        return c;

            return null;
        }

        public PgSqlCursor CreateCursor(IQuerySource querySource, string searchColumn, object searchId, Action<IDbLink> beforeCreateAction, Action<IDbLink> cleanUpAction)
        {
            lock (_synchRoot)
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

                    var cursorName = string.Format("cs_cursor_{0}", Interlocked.Increment(ref s_nextCursorIndex));
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

                    lock (_cursors)
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
            if (cursor.ConnectionContext != this)
                throw new ArgumentException("Cursor is not owned by this context.", nameof(cursor));

            if (index < 0 || count < 0 || (index + count) > cursor.RecordCount)
                throw new IndexOutOfRangeException();

            if (cursor.RecordCount == 0 || count == 0)
                return QueryResult.Empty;

            lock (_synchRoot)
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
            lock (_cursors)
                if (!_cursors.Remove(cursor))
                    return;

            lock (_synchRoot)
            {
                if (IsActive)
                {
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
                }

                if (_cursors.Count > 0)
                    return;

                if (Manager?.ShouldDispose(this) != true)
                    return;
            }

            Dispose();
        }

        internal void TryCreateConnection()
        {
            lock (_synchRoot)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(PgSqlCursorConnectionContext));

                if (_link != null)
                    return;

                _link = Manager.LinkProvider.CreateLink(DbLinkCreateOption.RequiresNew);
                _transaction = ((PgSqlLinkContext)_link.ConnectionContext).BeginForCursorManager();
                _transaction.Context.TransactionClosed += (_, _) => IsActive = false;

                IsActive = true;
                IsNew = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            PendingDisposeInternal();

            lock (_cursors)
                _cursors.Clear();

            lock (_synchRoot)
            {
                if (Manager != null)
                    try
                    { Manager?.OnConnectionContextDisposed(this); }
                    catch { /* ignored */ }
                    finally { Manager = null; }

                if (_transaction != null)
                    try
                    { _transaction?.Dispose(); }
                    catch { /* ignored */ }
                    finally { _transaction = null; }

                if (_link != null)
                    try
                    { _link?.Dispose(); }
                    catch { /* ignored */ }
                    finally { _link = null; }
            }
        }
        internal void PendingDisposeInternal()
        {
            _disposed = true;
            IsActive = false;
            IsNew = false;
        }

        private void ThrowIfNotActive()
        {
            if (!IsActive)
                throw new ObjectDisposedException("Cursor connection is closed.");
        }
    }
}
