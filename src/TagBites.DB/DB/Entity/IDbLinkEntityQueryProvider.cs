using System.Linq.Expressions;
using TagBites.Sql;

namespace TagBites.DB.Entity
{
    public interface IDbLinkEntityQueryProvider
    {
        SqlQuerySelect GetQuery(Expression expression);
    }
}
