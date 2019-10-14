using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlExpressionIndexerOperator : SqlExpression
    {
        public SqlExpression Operand { get; }
        public SqlExpression Index { get; }

        public SqlExpressionIndexerOperator(SqlExpression operand, SqlExpression index)
        {
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
            Index = index ?? throw new ArgumentNullException(nameof(index));
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
