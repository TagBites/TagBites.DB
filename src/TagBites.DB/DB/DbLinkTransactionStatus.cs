using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Data.DB
{
    public enum DbLinkTransactionStatus : byte
    {
        None,
        Pending,
        Open,
        Committing,
        RollingBack
    }
}
