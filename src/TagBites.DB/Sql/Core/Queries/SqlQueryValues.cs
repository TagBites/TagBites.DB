namespace TagBites.Sql
{
    public class SqlQueryValues : SqlQuerySelectBase
    {
        public SqlClauseValues Values { get; } = new SqlClauseValues();


        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitQuery(this, builder);
            resolver.PostVisitQuery(this, builder);
        }
    }
}
