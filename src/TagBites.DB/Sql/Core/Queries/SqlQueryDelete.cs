using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlQueryDelete : SqlQueryBase
    {
        public SqlClauseWith With { get; } = new SqlClauseWith();
        public SqlTable From { get; }
        public SqlClauseJoin Join { get; } = new SqlClauseJoin();
        public SqlClauseFrom Using { get; } = new SqlClauseFrom();
        public SqlClauseConditionals Where { get; } = new SqlClauseConditionals();
        public SqlClauseSelect Returning { get; } = new SqlClauseSelect();

        public SqlQueryDelete(string fromTableName)
            : this(fromTableName, null)
        { }
        public SqlQueryDelete(string fromTableName, string fromTableNameAlias)
        {
            Guard.ArgumentNotNullOrEmpty(fromTableName, nameof(fromTableName));
            From = new SqlTable(fromTableName, string.IsNullOrEmpty(fromTableNameAlias) ? "fdt_0" : fromTableNameAlias);
        }


        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitQuery(this, builder);
            resolver.PostVisitQuery(this, builder);
        }
    }
}
