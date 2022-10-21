using System;

namespace TagBites.Sql.TransactSql
{
    public class TransactSqlQueryResolver : SqlQueryResolver
    {
        protected internal override void VisitExpression(SqlExpressionFunctionCall expression, SqlQueryBuilder builder)
        {
            if (expression.FunctionName == nameof(SqlFunction.Trim))
                expression = (SqlExpressionFunctionCall)SqlFunction.TrimStart(expression);

            base.VisitExpression(expression, builder);
        }

        protected override string GetCastString(object value, string typeName)
        {
            return value is string
                ? $"CONVERT({typeName}, '{value}')"
                : $"CONVERT({typeName}, {value})";
        }

        protected override string GetBuildInFunctionName(string functionName)
        {
            if (string.Equals(functionName, nameof(SqlFunction.Length), StringComparison.OrdinalIgnoreCase))
                return "LEN";
            if (string.Equals(functionName, nameof(SqlFunction.TrimStart), StringComparison.OrdinalIgnoreCase))
                return "LTRIM";
            if (string.Equals(functionName, nameof(SqlFunction.TrimEnd), StringComparison.OrdinalIgnoreCase) || string.Equals(functionName, nameof(SqlFunction.Trim), StringComparison.OrdinalIgnoreCase))
                return "RTRIM";

            return base.GetBuildInFunctionName(functionName);
        }
    }
}
