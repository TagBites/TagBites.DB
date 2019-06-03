using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;
using static TBS.Sql.SqlExpression;

namespace TBS.Sql
{
    public static class SqlFunction
    {
        public static SqlExpression Custom(string functionName, params SqlExpression[] operands)
        {
            return Function(functionName, operands);
        }

        public static SqlExpression Abs(SqlExpression operand)
        {
            return Function("ABS", operand);
        }
        public static SqlExpression Round(SqlExpression operand)
        {
            return Function("ROUND", operand);
        }
        public static SqlExpression Round(SqlExpression operand, int decimalPlaces)
        {
            return Function("ROUND", operand, Argument(decimalPlaces));
        }

        /// <summary>
        /// The NULLIF function returns a null value if operand1 equals operand2; otherwise it returns operand1.
        /// </summary>
        public static SqlExpression NullIf(SqlExpression operand1, SqlExpression operand2)
        {
            return Function("NULLIF", operand1, operand2);
        }

        public static SqlExpression Length(SqlExpression operand)
        {
            return Function("LENGTH", operand);
        }
        public static SqlExpression Lower(SqlExpression operand)
        {
            return Function("LOWER", operand);
        }
        public static SqlExpression Upper(SqlExpression operand)
        {
            return Function("UPPER", operand);
        }
        public static SqlExpression Replace(SqlExpression operand, SqlExpression what, SqlExpression with)
        {
            return Function("REPLACE", operand, what, with);
        }
        public static SqlExpression Trim(SqlExpression operand)
        {
            return Function("TRIM", operand);
        }
        public static SqlExpression TrimStart(SqlExpression operand)
        {
            return Function("TRIMSTART", operand);
        }
        public static SqlExpression TrimEnd(SqlExpression operand)
        {
            return Function("TRIMEND", operand);
        }
        public static SqlExpression Substring(SqlExpression operand, int index)
        {
            return Function("SUBSTRING", operand, Argument(index));
        }
        public static SqlExpression Substring(SqlExpression operand, int index, int count)
        {
            return Function("SUBSTRING", operand, Argument(index), Argument(count));
        }

        public static SqlExpression Count()
        {
            return Function("COUNT");
        }
        public static SqlExpression Count(SqlExpression operand)
        {
            return Function("COUNT", operand);
        }
        public static SqlExpression Avg(SqlExpression operand)
        {
            return Function("AVG", operand);
        }
        public static SqlExpression Sum(SqlExpression operand)
        {
            return Function("SUM", operand);
        }
        public static SqlExpression Min(params SqlExpression[] operands)
        {
            Guard.ArgumentNotNullOrEmpty(operands, nameof(operands));
            return Function("MIN", operands);
        }
        public static SqlExpression Max(params SqlExpression[] operands)
        {
            Guard.ArgumentNotNullOrEmpty(operands, nameof(operands));
            return Function("MAX", operands);
        }

        public static SqlExpression Random()
        {
            return Function("RANDOM");
        }
    }
}
