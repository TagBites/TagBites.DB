using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB
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
    }
}
