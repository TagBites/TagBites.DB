using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql
{
    public enum SqlExpressionBinaryOperatorType
    {
        Plus,
        Minus,
        Divide,
        Modulo,
        Multiply,

        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        BitwiseLeftShift,
        BitwiseRightShift,

        Concat
    }
}