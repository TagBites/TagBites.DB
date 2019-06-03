using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TBS.Sql;

namespace TBS.DB.Entity
{
    public interface IDbLinkEntityQueryProvider
    {
        SqlQuerySelect GetQuery(Expression expression);
    }
}