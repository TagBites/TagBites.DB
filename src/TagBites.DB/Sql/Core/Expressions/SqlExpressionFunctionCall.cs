using System;
using System.Collections.Generic;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlExpressionFunctionCall : SqlExpression
    {
        public string FunctionName { get; }
        public IList<SqlExpression> Operands { get; }

        public SqlExpressionFunctionCall(string functionName)
        {
            Guard.ArgumentNotNullOrEmpty(functionName, nameof(functionName));

            FunctionName = functionName;
            Operands = Array.Empty<SqlExpression>();
        }
        public SqlExpressionFunctionCall(string functionName, IList<SqlExpression> operands)
        {
            Guard.ArgumentNotNullOrEmpty(functionName, nameof(functionName));
            Guard.ArgumentNotNullWithNotNullItems(operands, nameof(operands));

            FunctionName = functionName;
            Operands = operands;
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }
    }
}
