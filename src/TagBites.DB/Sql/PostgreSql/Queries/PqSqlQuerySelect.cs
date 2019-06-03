using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql.PostgreSql
{
    public class PqSqlQuerySelect : SqlQuerySelect
    {
        public PgSqlClauseLocking Locking { get; } = new PgSqlClauseLocking();
    }
}