using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB.Utils
{
    public enum DbTableChangerRecordStatus
    {
        Pending,
        NotFound,
        Inserted,
        Updated
    }
}
