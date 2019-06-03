using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBS.Sql;
using TBS.Utils;

namespace TBS.Data.DB
{
    public class DbQueryCursorManager : IDbCursorManager
    {
        private static int s_nextCursorIndex;

        private readonly object m_locker = new object();
        private readonly IDictionary<string, DbQueryCursor> m_cursors = new Dictionary<string, DbQueryCursor>();

        private readonly IDbLinkProvider m_linkProvider;
        private IDbLink m_link;

        public int CursorCount
        {
            get
            {
                lock (m_locker)
                    return m_cursors.Count;
            }
        }
        public IDbLinkProvider LinkProvider => m_linkProvider;

        public DbQueryCursorManager(IDbLinkProvider connectionProvider)
        {
            Guard.ArgumentNotNull(connectionProvider, nameof(connectionProvider));
            m_linkProvider = connectionProvider;
        }
        ~DbQueryCursorManager()
        {
            Dispose(false);
        }


        public bool ContainsCursor(string cursorName)
        {
            return GetCursor(cursorName) != null;
        }
        public IDbCursor GetCursor(string cursorName)
        {
            Guard.ArgumentNotNull(cursorName, nameof(cursorName));
            cursorName = cursorName.ToLower();

            lock (m_locker)
                return m_cursors.TryGetValue(cursorName, out var v) ? v : null;
        }

        public IDbCursor CreateCursor(IQuerySource querySource)
        {
            return CreateCursor(querySource, null, null, null);
        }
        public IDbCursor CreateCursor(IQuerySource querySource, string searchColumn, object searchId)
        {
            return CreateCursor(querySource, null, searchColumn, searchId);
        }
        public IDbCursor CreateCursor(IQuerySource querySource, IQuerySource queryCountSource)
        {
            return CreateCursor(querySource, queryCountSource, null, null);
        }
        public IDbCursor CreateCursor(IQuerySource querySource, IQuerySource queryCountSource, string searchColumn, object searchId)
        {
            return CreateCursor(querySource, queryCountSource, searchColumn, searchId, null, null);
        }
        public IDbCursor CreateCursor(IQuerySource querySource, IQuerySource queryCountSource, string searchColumn, object searchId, Action<IDbLink> beforeCreateAction, Action<IDbLink> cleanUpAction)
        {
            Guard.ArgumentNotNull(querySource, nameof(querySource));

            lock (m_locker)
            {
                var query = m_linkProvider.QueryResolver.GetQuery(querySource);
                var queryCount = queryCountSource != null ? m_linkProvider.QueryResolver.GetQuery(queryCountSource) : null;
                var cursor = CreateCursorInner(query, queryCount, searchColumn, searchId, beforeCreateAction, cleanUpAction);
                if (cursor == null)
                    return null;

                return m_cursors[cursor.Name] = cursor;
            }
        }

        public void Clear()
        {
            ClearLink();
        }

        internal QueryResult ExecuteInternal(DbQueryCursor cursor, int index, int count)
        {
            Guard.ArgumentNotNull(cursor, nameof(cursor));

            lock (m_locker)
            {
                if (GetCursor(cursor.Name) != cursor)
                    throw new Exception("Cursor is closed.");

                return ExecuteInner(cursor, index, count);
            }
        }
        internal void CloseCursorInternal(DbQueryCursor cursor)
        {
            Guard.ArgumentNotNull(cursor, nameof(cursor));

            lock (m_locker)
            {
                if (m_cursors.Remove(cursor.Name))
                {
                    try
                    {
                        CloseCursorInner(cursor.Name, cursor.CleanUpAction);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private DbQueryCursor CreateCursorInner(Query query, Query queryCount, string searchColumn, object searchId, Action<IDbLink> beforeCreateAction, Action<IDbLink> cleanUpAction)
        {
            ExecuteAction(beforeCreateAction);

            var cursorName = string.Format("cs_cursor_{0}", System.Threading.Interlocked.Increment(ref s_nextCursorIndex));
            int count;
            int? searchResultPosition = null;

            // Count
            {
                var q = new SqlQuerySelect();
                q.From.Add(queryCount ?? query);
                q.Select.Add(SqlFunction.Count(SqlExpression.Literal("*")));

                count = ExecuteScalar(m_linkProvider.QueryResolver.GetQuery(q));
            }

            // Search Result
            if (!string.IsNullOrEmpty(searchColumn) && searchId != null)
            {
                var q = new SqlQuerySelect();
                var f = q.From.Add(query);
                q.Select.Add(SqlExpression.Function("GetRowIndex", f.Column(searchColumn), SqlExpression.Argument(searchId)));

                searchResultPosition = ExecuteScalar(q);
            }

            return new DbQueryCursor(this, query, cursorName, count, cleanUpAction)
            {
                SearchResultPosition = searchResultPosition
            };
        }
        private QueryResult ExecuteInner(DbQueryCursor cursor, int index, int count)
        {
            if (index < 0 || count < 0 || (index + count) > cursor.RecordCount)
                throw new IndexOutOfRangeException();

            if (cursor.RecordCount == 0)
                return QueryResult.Empty;

            int ci = cursor.Position;
            int shift;

            if (index > ci)
            {
                shift = index - ci;
                ci += shift;
            }
            else
            {
                shift = ci - index;
                ci -= shift;
            }

            if (shift > 0)
                cursor.Position = ci;

            // Result
            if (count == 0)
                return QueryResult.Empty;


            var q = new SqlQuerySelect();
            q.Select.AddAll();
            q.From.Add(cursor.Query);
            q.Offset = index;
            q.Limit = count;

            var result = Execute(m_linkProvider.QueryResolver.GetQuery(q));
            cursor.Position += count;

            return result;
        }
        private void CloseCursorInner(string cursorName, Action<IDbLink> cleanUpAction)
        {
            try
            {
                ExecuteAction(cleanUpAction);
            }
            finally
            {
                if (CursorCount == 0)
                    ClearLink();
            }
        }

        private QueryResult Execute(IQuerySource query)
        {
            try
            {
                CheckLink();
                return m_link.Execute(query);
            }
            catch (Exception)
            {
                ClearLink();
                throw;
            }
        }
        private int ExecuteNonQuery(IQuerySource query)
        {
            try
            {
                CheckLink();
                return m_link.ExecuteNonQuery(query);
            }
            catch (Exception)
            {
                ClearLink();
                throw;
            }
        }
        private int ExecuteScalar(IQuerySource query)
        {
            try
            {
                CheckLink();
                return m_link.ExecuteScalar<int>(query);
            }
            catch (Exception)
            {
                ClearLink();
                throw;
            }
        }
        private void ExecuteAction(Action<IDbLink> action)
        {
            if (action == null)
                return;

            try
            {
                CheckLink();
                action(m_link);
            }
            catch (Exception)
            {
                ClearLink();
                throw;
            }
        }

        private void ClearLink()
        {
            lock (m_locker)
            {
                try
                {
                    if (m_link != null)
                        try
                        {
                            m_link.Dispose();
                        }
                        catch { }
                        finally { m_link = null; }
                }
                catch { }

                m_cursors.Clear();
            }
        }
        private void CheckLink()
        {
            if (m_link == null)
            {
                m_link = m_linkProvider.CreateLink(DbLinkCreateOption.RequiresNew);
                m_link.Force();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            ClearLink();
        }
    }
}
