using System;

namespace TagBites.DB
{
    public interface IDbLinkTransaction : IDisposable
    {
        IDbLinkContext ConnectionContext { get; }
        IDbLinkTransactionContext Context { get; }


        void Commit();
        void Rollback();
    }
}
