using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Sql
{
    public class SqlClauseReturningForInsert : SqlClauseSelect
    {
        public void Add(string column, string alias)
        {
            Add(new SqlColumn(new SqlTable(column), column), alias);
        }
    }
}
