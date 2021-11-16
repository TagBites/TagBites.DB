namespace TagBites.Sql
{
    public class SqlQueryInsertValues : SqlQueryInsertBase
    {
        public SqlClauseValues Values { get; } = new SqlClauseValues();

        public SqlQueryInsertValues(string intoTableName)
            : this(intoTableName, null)
        { }
        public SqlQueryInsertValues(string intoTableName, string intoTableNameAlias)
            : base(intoTableName, intoTableNameAlias)
        { }


        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitQuery(this, builder);
            resolver.PostVisitQuery(this, builder);
        }
    }
}
