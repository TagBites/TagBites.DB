using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlQuerySelect : SqlQuerySelectBase
    {
        public SqlClauseWith With { get; } = new SqlClauseWith();
        public SqlClauseSelect Select { get; } = new SqlClauseSelect();
        public SqlClauseDistinct Distinct { get; } = new SqlClauseDistinct();
        public SqlClauseFrom From { get; } = new SqlClauseFrom();
        public SqlClauseJoin Join { get; } = new SqlClauseJoin();
        public SqlClauseConditionals Where { get; } = new SqlClauseConditionals();
        public SqlClauseGroupBy GroupBy { get; } = new SqlClauseGroupBy();
        public SqlClauseConditionals Having { get; } = new SqlClauseConditionals();


        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitQuery(this, builder);
        }
    }
}
