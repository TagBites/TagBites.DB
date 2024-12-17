using System.Linq;

namespace TagBites.Sql
{
    public class SqlQuerySelect : SqlQuerySelectBase
    {
        public SqlClauseWith With { get; } = new();
        public SqlClauseSelect Select { get; } = new();
        public SqlClauseDistinct Distinct { get; } = new();
        public SqlClauseFrom From { get; } = new();
        public SqlClauseJoin Join { get; } = new();
        public SqlClauseConditionals Where { get; } = new();
        public SqlClauseGroupBy GroupBy { get; } = new();
        public SqlClauseConditionals Having { get; } = new();


        public void AddFilter(SqlQueryFilter filter)
        {
            With.AddRange(filter.With);
            Join.AddRange(filter.Join.Where(x => !Join.Contains(x)));
            Where.AddRange(filter);
        }

        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitQuery(this, builder);
            resolver.PostVisitQuery(this, builder);
        }
    }
}
