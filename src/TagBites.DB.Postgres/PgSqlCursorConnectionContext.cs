using System.Globalization;

namespace TagBites.DB.Postgres;

internal class PgSqlCursorConnectionContext : IDbCursorOwner
{
    private const string SearchFunctionName = "cursor_search_index";
    private const string HstoreFieldSql = "hstore(current_row) -> search_column";
    private const string JsonFieldSql = "to_jsonb(current_row) ->> search_column";

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

                var cursorName = string.Format("cs_cursor_{0}", Interlocked.Increment(ref s_nextCursorIndex));
                int rowCount;
                int searchIndex;

                if (string.IsNullOrEmpty(searchColumn) || searchId == null)
                {
                    rowCount = _link.ExecuteNonQuery(Query.Concat(
                        DeclareCursor(),
                        new Query($"MOVE FORWARD ALL IN {cursorName}"),
                        new Query($"MOVE ABSOLUTE 0 IN {cursorName}")));
                    searchIndex = -1;
                }
                else
                {
                    EnsureSearchFunction();

                    var searchQuery = new Query($"SELECT record_count, search_index FROM {SearchFunctionName}({{0}}, {{1}}, {{2}})",
                        cursorName, searchColumn, Convert.ToString(searchId, CultureInfo.InvariantCulture));

                    var statistics = _link.Execute(Query.Concat(DeclareCursor(), searchQuery));
                    rowCount = statistics.GetValue<int>(0, 0);
                    searchIndex = statistics.GetValue<int>(0, 1);
                }

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

                Query DeclareCursor() => new($"DECLARE {cursorName} SCROLL CURSOR FOR {query.Command}", query.Parameters);
            }
            catch
            {
                Dispose();
                throw;
            }

        }
    }
    private void EnsureSearchFunction()
    {
        var provider = (PgSqlLinkProvider)Manager.LinkProvider;
        if (provider.CursorSearchFunctionReadyInternal)
            return;

        using (var link = provider.CreateExclusiveLink())
        using (var transaction = link.Begin())
        {
            var functions = link.Execute($"""
                                          SELECT
                                              to_regprocedure('{SearchFunctionName}(text,text,text)') IS NOT NULL AS has_search,
                                              to_regprocedure('hstore(record)') IS NOT NULL AS has_hstore
                                          """);

            if (!functions.GetValue<bool>(0, 0))
                link.ExecuteNonQuery(GetSearchFunctionSql(functions.GetValue<bool>(0, 1) ? HstoreFieldSql : JsonFieldSql));

            transaction.Commit();
        }

        provider.CursorSearchFunctionReadyInternal = true;
    }
    private static string GetSearchFunctionSql(string fieldSql)
    {
        return $"""
                CREATE OR REPLACE FUNCTION {SearchFunctionName}(cursor_name text, search_column text, search_id text,
                                                                OUT record_count int, OUT search_index int)
                LANGUAGE plpgsql AS $$
                DECLARE
                    cur refcursor;
                    current_row record;
                    remaining int;
                BEGIN
                    cur := cursor_name;
                    record_count := 0;
                    search_index := -1;

                    LOOP
                        FETCH cur INTO current_row;
                        EXIT WHEN NOT FOUND;

                        IF {fieldSql} = search_id THEN
                            search_index := record_count;

                            -- Counting the rest without reading it costs about a microsecond less per row.
                            MOVE FORWARD ALL FROM cur;
                            GET DIAGNOSTICS remaining = ROW_COUNT;
                            record_count := record_count + 1 + remaining;
                            EXIT;
                        END IF;

                        record_count := record_count + 1;
                    END LOOP;

                    MOVE ABSOLUTE 0 FROM cur;
                END;
                $$
                """;
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

            try
            {
                _link = Manager.LinkProvider.CreateLink(DbLinkCreateOption.RequiresNew);
                _transaction = ((PgSqlLinkContext)_link.ConnectionContext).BeginForCursorManager();
                _transaction.Context.TransactionClosed += (_, _) => IsActive = false;
            }
            catch
            {
                Dispose();
                throw;
            }

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
