using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlClauseGroupByEntry : SqlClauseEntry
    {
        public SqlExpression Expression { get; }

        public SqlClauseGroupByEntry(SqlExpression expression)
        {
            Guard.ArgumentNotNull(expression, "expression");
            Expression = expression;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.Visit(Expression, builder);
        }
    }
}
