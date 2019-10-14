using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Sql;

namespace TagBites.DB
{
    public interface IQuerySource
    {
        Query CreateQuery(SqlQueryResolver resolver, SqlQueryBuilder builder);
    }
}
