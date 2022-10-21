namespace TagBites.DB.SqlServer;
public class SqlServerLinkProvider : DbLinkProvider
{
    public SqlServerLinkProvider(string connectionString)
        : this(new DbConnectionArguments(connectionString))
    { }
    public SqlServerLinkProvider(DbConnectionArguments arguments)
        : this(new SqlServerLinkAdapter(), arguments)
    { }
    public SqlServerLinkProvider(SqlServerLinkAdapter adapter, string connectionString)
        : this(adapter, new DbConnectionArguments(connectionString))
    { }
    public SqlServerLinkProvider(DbLinkAdapter linkAdapter, DbConnectionArguments arguments)
        : base(linkAdapter, arguments)
    {
    }
}
