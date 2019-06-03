using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql
{
    public enum SqlConditionUnaryOperatorType
    {
        Not,
        Exists,
        NotExists,
        IsNull,
        IsNotNull
    }
}