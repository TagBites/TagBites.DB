using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TagBites.Sql;
using TagBites.Sql.Postgres;

namespace TagBites.DB.Postgres
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public abstract class PgSqlLinkAdapter : DbLinkAdapter
    {
        public override SqlQueryResolver QueryResolver { get; } = new PqSqlQueryResolver();
        public override int DefaultPort => 5432;


        protected override DbLink CreateDbLink() => new PgSqlLink();
    }
}
