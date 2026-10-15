using TagBites.Utils;

namespace TagBites.Sql;

public class SqlExpressionFieldOperator : SqlExpression
{
    public SqlExpression Operand { get; }
    public string FieldName { get; }

    public SqlExpressionFieldOperator(SqlExpression operand, string fieldName)
    {
        Guard.ArgumentNotNull(operand, nameof(operand));
        Guard.ArgumentNotNullOrEmpty(fieldName, nameof(fieldName));

        Operand = operand;
        FieldName = fieldName;
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
