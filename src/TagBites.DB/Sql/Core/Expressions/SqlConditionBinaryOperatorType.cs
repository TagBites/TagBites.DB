namespace TagBites.Sql
{
    public enum SqlConditionBinaryOperatorType
    {
        Equal,
        NotEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,

        Like,
        Contains,
        StartsWith,
        EndsWith,

        Distinct,
        NotDistinct,

        ILike
    }
}
