using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql
{
    public enum SqlConditionBinaryOperatorType
    {
        Equal,
        NotEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,

        Like,
        Contains,
        StartsWith,
        EndsWith,

        Distinct,
        NotDistinct,
    }
}
