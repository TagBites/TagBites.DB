using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlClauseJoinEntry : SqlClauseEntry
    {
        public SqlClauseJoinEntryType JoinType { get; }
        public SqlTable Table { get; }
        public SqlClauseJoinEntryConditionType ConditionType { get; }
        public SqlExpression Condition { get; }

        public SqlClauseJoinEntry(SqlClauseJoinEntryType join, SqlTable table, SqlClauseJoinEntryConditionType conditionType, SqlExpression condition)
        {
            Guard.ArgumentNotNull(table, "table");
            Guard.ArgumentNotNull(condition, "condition");

            JoinType = join;
            Table = table;
            ConditionType = conditionType;
            Condition = condition;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClauseEntry(this, builder);
        }

        protected bool Equals(SqlClauseJoinEntry other)
        {
            return JoinType == other.JoinType && Equals(Table, other.Table) && ConditionType == other.ConditionType && Equals(Condition, other.Condition);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SqlClauseJoinEntry)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)JoinType;
                hashCode = (hashCode * 397) ^ Table.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)ConditionType;
                hashCode = (hashCode * 397) ^ Condition.GetHashCode();
                return hashCode;
            }
        }
    }
}
