using System;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlTable : SqlExpression
    {
        public object Table { get; private set; }
        public string Alias { get; internal set; }

        protected SqlTable(string table)
        {
            Guard.ArgumentNotNullOrEmpty(table, "table");
            Table = table;
        }
        protected SqlTable(object table)
        {
            Guard.ArgumentNotNull(table, "table");
            Table = table;
        }
        public SqlTable(string table, string alias)
            : this((object)table, alias)
        {
            Guard.ArgumentNotNullOrEmpty(table, "table");
        }
        public SqlTable(SqlQueryBase table, string alias)
            : this((object)table, alias)
        { }
        public SqlTable(SqlExpressionQuery expression, string alias)
            : this((object)expression, alias)
        { }
        public SqlTable(SqlLiteralExpression expression, string alias)
            : this((object)expression, alias)
        { }
        public SqlTable(SqlLiteral table, string alias)
            : this((object)table, alias)
        { }
        private SqlTable(object table, string alias)
        {
            Guard.ArgumentNotNull(table, "table");
            Guard.ArgumentNotNullOrEmpty(alias, "alias");

            Table = table;
            Alias = alias;
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }

        public TTable As<TTable>() where TTable : SqlTable, new()
        {
            return new TTable
            {
                Table = Table,
                Alias = Alias
            };
        }
        public SqlColumn Column(string columnName)
        {
            return new SqlColumn(this, columnName);
        }

        protected bool Equals(SqlTable other) => Equals(Table, other.Table) && string.Equals(Alias, other.Alias);
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is SqlTable other && Equals(other);
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            unchecked { return Table.GetHashCode() * 397; }
        }

        [Obsolete("Please use Column(string columnName).", false)]
        public SqlColumn GetColumn(string columnName)
        {
            return new SqlColumn(this, columnName);
        }
    }
}
