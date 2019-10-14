using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB.Postgres
{
    public class PgSqlCursor : IDbCursor
    {
        private PgSqlCursorConnectionContext _context;

        public IDbCursorOwner Owner => _context;
        public DateTime CreateDateTime { get; }
        public DateTime LastExecuteDateTime { get; private set; }
        public Query Query { get; }
        public string Name { get; }
        public int RecordCount { get; }
        public int Position { get; internal set; }

        public int? SearchResultPosition { get; set; }

        internal Action<IDbLink> CleanUpAction { get; }
        internal PgSqlCursorConnectionContext ConnectionContext => _context;

        internal PgSqlCursor(PgSqlCursorConnectionContext context, Query query, string name, int rowCount, int searchIndex, Action<IDbLink> cleanUpAction)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            _context = context;
            Query = query;
            Name = name;
            RecordCount = rowCount;
            SearchResultPosition = searchIndex >= 0 ? searchIndex : (int?)null;
            CleanUpAction = cleanUpAction;

            CreateDateTime = DateTime.Now;
            LastExecuteDateTime = DateTime.Now;
        }


        public QueryResult Execute(int index, int count)
        {
            return Execute(index, count, false);
        }
        public QueryResult Execute(int index, int count, bool forceExecute)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            CheckDispose();

            if (index + count > RecordCount)
                count = RecordCount - index;

            if (!forceExecute && (count == 0 || index >= RecordCount))
                return QueryResult.Empty;

            LastExecuteDateTime = DateTime.Now;
            return _context.FetchCursor(this, index, count);
        }

        private void CheckDispose()
        {
            if (_context == null)
                throw new ObjectDisposedException("DbLinkCursor");
        }
        public void Dispose()
        {
            if (_context != null)
            {
                _context.CloseCursor(this);
                _context = null;
            }
        }
    }
}
