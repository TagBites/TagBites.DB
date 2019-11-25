namespace TagBites.Sql.Postgres
{
    public enum PgSqlClauseLockingEntryWaitMode
    {
        Default,
        Nowait,
        SkipLocked
    }
}
