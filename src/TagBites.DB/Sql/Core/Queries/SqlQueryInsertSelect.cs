using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql
{
    public class SqlQueryInsertSelect : SqlQueryInsertBase
    {
        public SqlQuerySelect Select { get; }

        public SqlQueryInsertSelect(string intoTableName)
            : this(intoTableName, null, null)
        { }
        public SqlQueryInsertSelect(string intoTableName, string intoTableNameAlias)
            : this(intoTableName, intoTableNameAlias, null)
        { }
        public SqlQueryInsertSelect(string intoTableName, SqlQuerySelect select)
            : this(intoTableName, null, select)
        { }
        public SqlQueryInsertSelect(string intoTableName, string intoTableNameAlias, SqlQuerySelect select)
            : base(intoTableName, intoTableNameAlias)
        {
            Select = select ?? new SqlQuerySelect();
        }


        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitQuery(this, builder);
        }
    }
}
