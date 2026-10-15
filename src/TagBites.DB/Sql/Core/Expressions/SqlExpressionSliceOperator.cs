using TagBites.Utils;

namespace TagBites.Sql;

public class SqlExpressionSliceOperator : SqlExpression
{
    public SqlExpression Operand { get; }
    public SqlExpression LowerBound { get; }
    public SqlExpression UpperBound { get; }

    public SqlExpressionSliceOperator(SqlExpression operand, SqlExpression lowerBound, SqlExpression upperBound)
    {
        Guard.ArgumentNotNull(operand, nameof(operand));
        Guard.ArgumentNotNull(lowerBound, nameof(lowerBound));
        Guard.ArgumentNotNull(upperBound, nameof(upperBound));

        Operand = operand;
        LowerBound = lowerBound;
        UpperBound = upperBound;
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
