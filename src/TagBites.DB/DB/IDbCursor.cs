using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Data.DB
{
    public interface IDbCursor : IDisposable
    {
        IDbCursorOwner Owner { get; }
        string Name { get; }
        Query Query { get; }
        int Position { get; }
        int RecordCount { get; }

        int? SearchResultPosition { get; }


        QueryResult Execute(int index, int count);
        QueryObjectResult<T> Execute<T>(int index, int count);

        IEnumerable<T> Iterate<T>();
        IEnumerable<T> Iterate<T>(int pageSize);
    }
}
