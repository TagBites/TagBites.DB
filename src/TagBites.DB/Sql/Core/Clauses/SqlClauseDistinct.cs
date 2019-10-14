using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlClauseDistinct : SqlClauseCollectionBase<SqlExpression>
    {
        private bool _enabled;

        public bool Enabled
        {
            get => _enabled || Count > 0;
            set => _enabled = value;
        }


        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, builder);
        }
    }
}