using System;
using System.ComponentModel;
using System.Text;

namespace TBS.Sql
{
    public class SqlQueryValues : SqlQuerySelectBase
    {
        public SqlClauseValues Values { get; } = new SqlClauseValues();


        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitQuery(this, builder);
        }
    }
}
