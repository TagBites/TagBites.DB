using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Sql.Postgres.Clauses;

namespace TagBites.Sql.Postgres.Queries
{
    public class PqSqlQuerySelect : SqlQuerySelect
    {
        public PgSqlClauseLocking Locking { get; } = new PgSqlClauseLocking();
    }
}