using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlExpressionFunctionCall : SqlExpression
    {
        public string FunctionName { get; }
        public IList<SqlExpression> Operants { get; }

        public SqlExpressionFunctionCall(string functionName)
        {
            Guard.ArgumentNotNullOrEmpty(functionName, nameof(functionName));

            FunctionName = functionName;
            Operants = EmptyExpressionArray;
        }
        public SqlExpressionFunctionCall(string functionName, IList<SqlExpression> operands)
        {
            Guard.ArgumentNotNullOrEmpty(functionName, nameof(functionName));
            Guard.ArgumentNotNullWithNotNullItems(operands, nameof(operands));

            FunctionName = functionName;
            Operants = operands;
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
