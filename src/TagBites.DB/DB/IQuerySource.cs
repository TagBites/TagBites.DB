using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Sql;

namespace TBS.Data.DB
{
    public interface IQuerySource
    {
        Query CreateQuery(SqlQueryResolver resolver, SqlQueryBuilder builder);
    }
}
