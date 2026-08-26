namespace TagBites.Sql.Resolvers;

internal class PostgresResolver : Postgres.PqSqlQueryResolver
{
    public new string GetCastString(object value, string typeName) => base.GetCastString(value, typeName);
}
