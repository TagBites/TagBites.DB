namespace TagBites.Sql.Resolvers;

internal class TransactSqlResolver : TransactSql.TransactSqlQueryResolver
{
    public new string GetCastString(object value, string typeName) => base.GetCastString(value, typeName);
}
