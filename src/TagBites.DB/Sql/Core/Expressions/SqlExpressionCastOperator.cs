using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlExpressionCastOperator : SqlExpression
    {
        public SqlExpression Operand { get; }
        public Type NetType { get; }
        //public DbType? DbType { get; }
        public string DbTypeName { get; }

        public SqlExpressionCastOperator(SqlExpression operand, Type netType)
        {
            Guard.ArgumentNotNull(operand, nameof(operand));
            Guard.ArgumentNotNull(netType, nameof(netType));

            NetType = netType;
            Operand = operand;
        }
        //public SqlExpressionCastOperator(SqlExpression operand, DbType dbType)
        //{
        //    Guard.ArgumentNotNull(operand, nameof(operand));

        //    DbType = dbType;
        //    Operand = operand;
        //}
        public SqlExpressionCastOperator(SqlExpression operand, string dbTypeName)
        {
            Guard.ArgumentNotNull(operand, nameof(operand));
            Guard.ArgumentNotNullOrEmpty(dbTypeName, nameof(dbTypeName));

            DbTypeName = dbTypeName;
            Operand = operand;
        }


        public override void Accept(ISqlExpressionVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitExpression(this, builder);
        }

        protected bool Equals(SqlExpressionCastOperator other)
        {
            return Equals(Operand, other.Operand) && NetType == other.NetType /*&& DbType == other.DbType*/ && DbTypeName == other.DbTypeName;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlExpressionCastOperator)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Operand.GetHashCode();
                hashCode = (hashCode * 397) ^ (NetType != null ? NetType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DbTypeName != null ? DbTypeName.GetHashCode() : 0);
                //hashCode = (hashCode * 397) ^ DbType.GetHashCode();
                return hashCode;
            }
        }
    }
}
