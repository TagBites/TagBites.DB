namespace TagBites.Sql.Postgres
{
    public class PqSqlQuerySelect : SqlQuerySelect
    {
        public PgSqlClauseLocking Locking { get; } = new PgSqlClauseLocking();
    }
}
