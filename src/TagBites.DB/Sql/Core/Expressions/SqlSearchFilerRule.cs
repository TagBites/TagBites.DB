using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlSearchFilerRule
    {
        public SqlExpression ColumnExpression { get; }
        public string AdjustFunction { get; }

        public SqlSearchFilerRule(SqlExpression columnExpression, string adjustFunction)
        {
            Guard.ArgumentNotNull(columnExpression, "columnExpression");

            ColumnExpression = columnExpression;
            AdjustFunction = adjustFunction;
        }
    }
}