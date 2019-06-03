using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBS.Utils;

namespace TBS.Data.DB
{
    public class DbQueryCursor : IDbCursor
    {
        private DbQueryCursorManager m_cursorManager;

        public IDbCursorOwner Owner => m_cursorManager;
        public DateTime CreateDateTime { get; private set; }
        public DateTime LastExecuteDateTime { get; private set; }
        public Query Query { get; private set; }
        public string Name { get; }
        public int RecordCount { get; }
        public int Position { get; internal set; }

        public int? SearchResultPosition { get; internal set; }

        internal Action<IDbLink> CleanUpAction { get; }

        internal DbQueryCursor(DbQueryCursorManager cursorManager, Query query, string name, int rowCount, Action<IDbLink> cleanUpAction)
        {
            Guard.ArgumentNotNull(cursorManager, nameof(cursorManager));
            Guard.ArgumentNotNull(name, nameof(name));
            Guard.ArgumentNotNull(query, nameof(query));

            m_cursorManager = cursorManager;
            Query = query;
            Name = name;
            RecordCount = rowCount;
            CleanUpAction = cleanUpAction;

            CreateDateTime = DateTime.Now;
            LastExecuteDateTime = DateTime.Now;
        }
        ~DbQueryCursor()
        {
            Dispose();
        }


        public QueryResult Execute(int index, int count)
        {
            return Execute(index, count, false);
        }
        public QueryResult Execute(int index, int count, bool forceExecute)
        {
            Guard.ArgumentNonNegative(index, nameof(index));
            Guard.ArgumentNonNegative(count, nameof(count));
            CheckDispose();

            if (index + count > RecordCount)
                count = RecordCount - index;

            if (!forceExecute && (count == 0 || index >= RecordCount))
                return QueryResult.Empty;

            LastExecuteDateTime = DateTime.Now;
            return m_cursorManager.ExecuteInternal(this, index, count);
        }

        public QueryObjectResult<T> Execute<T>(int index, int count)
        {
            return new QueryObjectResult<T>(Execute(index, count));
        }
        public QueryObjectResult<T> Execute<T>(int index, int count, QueryObjectResultPropertyResolver customPropertyResolver)
        {
            return new QueryObjectResult<T>(Execute(index, count), customPropertyResolver);
        }

        public IEnumerable<T> Iterate<T>()
        {
            return Iterate<T>(50, null);
        }
        public IEnumerable<T> Iterate<T>(int pageSize)
        {
            return Iterate<T>(pageSize, null);
        }
        public IEnumerable<T> Iterate<T>(int pageSize, QueryObjectResultPropertyResolver customPropertyResolver)
        {
            for (int i = 0; i < RecordCount; i += pageSize)
            {
                var result = Execute(i, pageSize);
                var objectResult = new QueryObjectResult<T>(result, customPropertyResolver);

                foreach (var item in objectResult)
                    yield return item;
            }
        }

        private void CheckDispose()
        {
            if (m_cursorManager == null)
                throw new ObjectDisposedException("DbQueryCursor");
        }
        public void Dispose()
        {
            if (m_cursorManager != null)
            {
                m_cursorManager.CloseCursorInternal(this);
                m_cursorManager = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
