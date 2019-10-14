using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TagBites.Sql;

namespace TagBites.DB.Entity
{
    public interface IDbLinkEntityQueryProvider
    {
        SqlQuerySelect GetQuery(Expression expression);
    }
}
