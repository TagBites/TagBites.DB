using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
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