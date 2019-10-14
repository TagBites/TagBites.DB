using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB
{
    public interface IDbLinkTransactionContext
    {
        DbLinkBag Bag { get; }
        Exception Exception { get; }
        int NestingLevel { get; }
        bool Started { get; }
        bool SystemTransaction { get; set; }

        event EventHandler TransactionBeforeBegin;
        event EventHandler TransactionBegin;
        event DbLinkTransactionCloseEventHandler TransactionClose;
    }
}
