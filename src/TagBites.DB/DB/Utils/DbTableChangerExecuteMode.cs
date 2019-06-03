using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Data.DB.Utils
{
    public enum DbTableChangerExecuteMode
    {
        Insert,
        Update,
        InsertOrUpdateBasedOnId,
        InsertOrUpdateBasedOnExistence
    }
}