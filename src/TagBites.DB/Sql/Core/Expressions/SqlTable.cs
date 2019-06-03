using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlTable : SqlExpression
    {
        public object Table { get; }
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

        public SqlColumn Column(string columnName)
        {
            return new SqlColumn(this, columnName);
        }

        protected bool Equals(SqlTable other)
        {
            return Equals(Table, other.Table) && string.Equals(Alias, other.Alias);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlTable)obj);
        }
        public override int GetHashCode()
        {
            unchecked { return (Table.GetHashCode() * 397) ^ (Alias != null ? Alias.GetHashCode() : 0); }
        }

        [Obsolete("Please use Column(string columnName).", false)]
        public SqlColumn GetColumn(string columnName)
        {
            return new SqlColumn(this, columnName);
        }
    }
}
