using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Data.DB
{
    public interface IDbLinkTransaction : IDisposable
    {
        IDbLinkTransactionContext Context { get; }

        void Commit();
        void Rollback();
    }
}
