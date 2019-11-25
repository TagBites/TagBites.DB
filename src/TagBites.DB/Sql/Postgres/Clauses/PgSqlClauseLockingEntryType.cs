namespace TagBites.Sql.Postgres
{
    public enum PgSqlClauseLockingEntryType
    {
        Update,
        NoKeyUpdate,
        Share,
        KeyShare
    }
}
