﻿using System;
using System.Collections.Generic;

namespace TBS.Data.DB.PostgreSql
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
            return Iterate<T>(500, null);
        }
        public IEnumerable<T> Iterate<T>(int pageSize)
        {
            return Iterate<T>(pageSize, null);
        }
        public IEnumerable<T> Iterate<T>(int pageSize, QueryObjectResultPropertyResolver customPropertyResolver)
        {
            for (var i = 0; i < RecordCount; i += pageSize)
            {
                var result = Execute(i, pageSize);
                var objectResult = new QueryObjectResult<T>(result, customPropertyResolver);

                foreach (var item in objectResult)
                    yield return item;
            }
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
