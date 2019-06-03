using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlColumn : SqlExpression
    {
        public SqlTable Table { get; }
        public string ColumnName { get; }

        public SqlColumn(SqlTable table, string columnName)
        {
            Guard.ArgumentNotNull(table, nameof(table));
            Guard.ArgumentNotNullOrEmpty(columnName, nameof(columnName));

            Table = table;
            ColumnName = columnName;
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }

        protected bool Equals(SqlColumn other)
        {
            return Equals(Table, other.Table) && string.Equals(ColumnName, other.ColumnName);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlColumn)obj);
        }
        public override int GetHashCode()
        {
            unchecked { return (Table.GetHashCode() * 397) ^ ColumnName.GetHashCode(); }
        }
    }
}
