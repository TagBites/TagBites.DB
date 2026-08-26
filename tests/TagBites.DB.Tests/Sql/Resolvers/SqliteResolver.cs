namespace TagBites.Sql.Resolvers;

internal class SqliteResolver : Sqlite.SqliteQueryResolver
{
    public new string GetCastString(object value, string typeName) => base.GetCastString(value, typeName);
}
