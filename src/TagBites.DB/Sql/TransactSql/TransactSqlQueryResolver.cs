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

        protected internal override bool NeedQuoteIdentifier(string name)
        {
            return !IsQuoted(name) && base.NeedQuoteIdentifier(name);
        }
        protected internal override string QuoteIdentifierIfNeeded(string name)
        {
            return NeedQuoteIdentifier(name)
                ? QuoteIdentifier(name)
                : name;
        }
        private static bool IsQuoted(string name) => name.Length > 1 && name[0] == '[' && name[name.Length - 1] == ']';

        protected override string GetCastString(object value, string typeName)
        {
            return value is string text
                ? $"CONVERT({typeName}, {ToEscapedString(text)})"
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
