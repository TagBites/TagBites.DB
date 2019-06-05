using System.ComponentModel;
using TBS.Sql;
using TBS.Sql.PostgreSql;

namespace TBS.Data.DB.PostgreSql
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public abstract class PgSqlLinkAdapter : DbLinkAdapter
    {
        public override SqlQueryResolver QueryResolver { get; } = new PqSqlQueryResolver();
        public override int DefaultPort => 5432;


        protected internal override DbLink CreateDbLink() => new PgSqlLink();
    }
}
