using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using TBS.Data.DB.Schema;
using TBS.DB.Entity;
using TBS.Sql;

namespace TBS.Data.DB
{
    public interface IDbLink : IDisposable
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsDisposed { get; }

        IDbLinkContext ConnectionContext { get; }
        IDbLinkTransactionContext TransactionContext { get; }
        DbLinkTransactionStatus TransactionStatus { get; }


        void Force();
        IDbLinkTransaction Begin();

        int ExecuteNonQuery(IQuerySource query);
        QueryResult Execute(IQuerySource query);
        object ExecuteScalar(IQuerySource query);

        QueryResult[] BatchExecute(IQuerySource query);
        DelayedBatchQueryResult DelayedBatchExecute(IQuerySource query);

        [EditorBrowsable(EditorBrowsableState.Never)]
        T ExecuteOnAdapter<T>(IQuerySource query, Func<DbDataAdapter, T> executor);
        [EditorBrowsable(EditorBrowsableState.Never)]
        T ExecuteOnReader<T>(IQuerySource query, Func<DbDataReader, T> executor);
    }
}
