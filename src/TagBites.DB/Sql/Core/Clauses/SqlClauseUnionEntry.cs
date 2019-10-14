using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlClauseUnionEntry : SqlClauseEntry
    {
        public SqlQuerySelect Select { get; }
        public SqlClauseUnionEntryType Type { get; }

        public SqlClauseUnionEntry(SqlQuerySelect select, SqlClauseUnionEntryType type)
        {
            Guard.ArgumentNotNull(select, "select");
            Select = select;
            Type = type;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClauseEntry(this, builder);
        }
    }

    public enum SqlClauseUnionEntryType
    {
        Default,
        All
    }
}
