using System;
using System.Linq;

namespace TagBites.Sql
{
    public class SqlLiteralExpression : SqlExpression
    {
        public string Format { get; }
        public SqlExpression[] Args { get; }

        public SqlLiteralExpression(string sqlLiteral)
            : this(sqlLiteral, null)
        { }
        public SqlLiteralExpression(string format, SqlExpression[] args)
        {
            Format = format;
            Args = args ?? Array.Empty<SqlExpression>();
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }

        protected bool Equals(SqlLiteralExpression other)
        {
            return string.Equals(Format, other.Format) && Args.SequenceEqual(other.Args);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlLiteralExpression)obj);
        }
        public override int GetHashCode()
        {
            unchecked { return (Format.GetHashCode() * 397) ^ Args.Length; }
        }
    }
}
