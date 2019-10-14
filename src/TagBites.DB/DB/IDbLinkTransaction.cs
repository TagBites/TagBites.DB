using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB
{
    public interface IDbLinkTransaction : IDisposable
    {
        IDbLinkTransactionContext Context { get; }

        void Commit();
        void Rollback();
    }
}
