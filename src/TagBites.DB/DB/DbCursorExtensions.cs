using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB
{
    public static class DbCursorExtensions
    {
        public static IList<T> AsList<T>(this IDbCursor cursor) where T : class
        {
            return new DbCursorList<T>(cursor, false);
        }

        public static QueryObjectResult<T> Execute<T>(this IDbCursor cursor, int index, int count)
        {
            return new QueryObjectResult<T>(cursor.Execute(index, count));
        }
        public static QueryObjectResult<T> Execute<T>(this IDbCursor cursor, int index, int count, QueryObjectResultPropertyResolver customPropertyResolver)
        {
            return new QueryObjectResult<T>(cursor.Execute(index, count), customPropertyResolver);
        }
    }
}
