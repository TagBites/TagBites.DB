using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Data.DB
{
    public interface IDbCursorManager : IDisposable, IDbCursorOwner
    {
        IDbCursor GetCursor(string cursorName);

        IDbCursor CreateCursor(IQuerySource querySource);
        IDbCursor CreateCursor(IQuerySource querySource, string searchColumn, object searchId);
        IDbCursor CreateCursor(IQuerySource querySource, IQuerySource queryCountSource, string searchColumn, object searchId, Action<IDbLink> beforeCreateAction, Action<IDbLink> cleanUpAction);
    }
}
