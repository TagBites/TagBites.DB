using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public abstract class SqlCondition : SqlExpression
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new SqlCondition ToCondition() => this;

        public static SqlCondition operator !(SqlCondition expression)
        {
            return Not(expression);
        }
        public static SqlCondition operator |(SqlCondition left, SqlCondition right) { return Or(left, right); }
        public static SqlCondition operator &(SqlCondition left, SqlCondition right) { return And(left, right); }
    }
}
