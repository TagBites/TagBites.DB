using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Collections.ObjectModel;
using TBS.Utils;

namespace TBS.Data.DB
{
    public class DbCursorList<T> : LazyList<T>, IDisposable
        where T : class
    {
        private readonly bool m_ownCursor;

        public IDbCursor Cursor { get; private set; }

        public DbCursorList(IDbCursor cursor, bool ownCursor = true)
            : base(cursor.RecordCount)
        {
            Guard.ArgumentNotNull(cursor, nameof(cursor));
            Cursor = cursor;
            m_ownCursor = ownCursor;

            LoadWindowSize = cursor.Owner.LinkProvider.Configuration.DefaultWindowSize;
        }
        ~DbCursorList()
        {
            Dispose();
        }


        protected override int LoadCore(ref T[] collection, int index, int count)
        {
            var items = Cursor.Execute<T>(index, count);

            if (collection == null)
                collection = new T[Cursor.RecordCount];

            for (var i = 0; i < count; i++)
                collection[index + i] = items[i];

            return Math.Min(count, collection.Length);
        }
        protected override void OnLoaded()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Cursor != null)
            {
                try
                {
                    if (m_ownCursor)
                        Cursor.Dispose();
                }
                finally { Cursor = null; }
            }
        }
    }
}
