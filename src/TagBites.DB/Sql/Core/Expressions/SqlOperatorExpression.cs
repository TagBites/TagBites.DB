using System;

namespace TagBites.Sql;

public class SqlOperatorExpression : SqlExpression
{
    public Enum Operator { get; }

    public SqlOperatorExpression(SqlConditionBinaryOperatorType opr) => Operator = opr;
    public SqlOperatorExpression(SqlConditionGroupOperatorType opr) => Operator = opr;
    public SqlOperatorExpression(SqlConditionUnaryOperatorType opr) => Operator = opr;
    public SqlOperatorExpression(SqlExpressionBinaryOperatorType opr) => Operator = opr;
    public SqlOperatorExpression(SqlExpressionUnaryOperatorType opr) => Operator = opr;


    public override void Accept(ISqlExpressionVisitor visitor)
    {
        visitor.VisitExpression(this);
    }
    protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
    {
        resolver.VisitExpression(this, builder);
    }
    public override string ToString()
    {
        return SqlQueryResolver.DefaultToStringResolver.GetOperatorTypeString(Operator);
    }

    protected bool Equals(SqlOperatorExpression other)
    {
        return string.Equals(Operator, other.Operator);
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((SqlOperatorExpression)obj);
    }
    public override int GetHashCode()
    {
        return Operator.GetHashCode();
    }
}
