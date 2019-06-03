using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql.TransactSql
{
    public class TransactSqlQueryResolver : SqlQueryResolver
    {
        protected internal override void VisitExpression(SqlExpressionFunctionCall expression, SqlQueryBuilder builder)
        {
            if (expression.FunctionName == nameof(SqlFunction.Trim))
                expression = (SqlExpressionFunctionCall)SqlFunction.TrimStart(expression);

            base.VisitExpression(expression, builder);
        }

        protected override string GetBuildInFunctionName(string functionName)
        {
            if (String.Equals(functionName, nameof(SqlFunction.Length), StringComparison.OrdinalIgnoreCase))
                return "LEN";
            else if (String.Equals(functionName, nameof(SqlFunction.TrimStart), StringComparison.OrdinalIgnoreCase))
                return "LTRIM";
            else if (String.Equals(functionName, nameof(SqlFunction.TrimEnd), StringComparison.OrdinalIgnoreCase) || String.Equals(functionName, nameof(SqlFunction.Trim), StringComparison.OrdinalIgnoreCase))
                return "RTRIM";

            return base.GetBuildInFunctionName(functionName);
        }
    }
}
