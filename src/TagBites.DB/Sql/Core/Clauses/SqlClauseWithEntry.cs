using System;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlClauseWithEntry : SqlClauseEntry
    {
        public string Name { get; }
        public string[] Columns { get; }
        public object Query { get; }

        public SqlClauseUnionEntryType RecursiveUnionType { get; private set; }
        public object RecursiveQuery { get; private set; }

        public SqlClauseWithEntry(string name, string[] columns, SqlQueryBase query)
        {
            Guard.ArgumentNotNullOrEmpty(name, "name");
            Guard.ArgumentNotNull(query, "builder");

            Name = name;
            Columns = columns ?? new string[0];
            Query = query;
        }
        public SqlClauseWithEntry(string name, string[] columns, SqlLiteral select)
        {
            Guard.ArgumentNotNullOrEmpty(name, "name");
            Guard.ArgumentNotNull(select, "select");

            Name = name;
            Columns = columns ?? new string[0];
            Query = select;
        }
        public SqlClauseWithEntry(string name, string[] columns, SqlLiteralExpression select)
        {
            Guard.ArgumentNotNullOrEmpty(name, "name");
            Guard.ArgumentNotNull(select, "select");

            Name = name;
            Columns = columns ?? new string[0];
            Query = select;
        }
        public SqlClauseWithEntry(string name, string[] columns, SqlExpressionQuery select)
        {
            Guard.ArgumentNotNullOrEmpty(name, "name");
            Guard.ArgumentNotNull(select, "select");

            Name = name;
            Columns = columns ?? new string[0];
            Query = select;
        }


        public SqlClauseWithEntry Recursive(SqlQueryBase query, SqlClauseUnionEntryType unionType) => Recursive((object)query, unionType);
        public SqlClauseWithEntry Recursive(SqlLiteral query, SqlClauseUnionEntryType unionType) => Recursive((object)query, unionType);
        public SqlClauseWithEntry Recursive(SqlLiteralExpression query, SqlClauseUnionEntryType unionType) => Recursive((object)query, unionType);
        public SqlClauseWithEntry Recursive(SqlExpressionQuery query, SqlClauseUnionEntryType unionType) => Recursive((object)query, unionType);
        private SqlClauseWithEntry Recursive(object query, SqlClauseUnionEntryType unionType)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (RecursiveQuery != null)
                throw new InvalidOperationException("Recursive has been declared already.");

            RecursiveQuery = query;
            RecursiveUnionType = unionType;

            return this;
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClauseEntry(this, builder);
        }
    }
}
