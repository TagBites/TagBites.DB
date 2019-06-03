using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Data.DB
{
    public interface IDbCursorOwner
    {
        IDbLinkProvider LinkProvider { get; }
    }
}
