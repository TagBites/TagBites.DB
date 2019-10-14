using System;

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
    }
}
