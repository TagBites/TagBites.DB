using System.Data.Common;
using System.Data.SqlClient;
using TagBites.Sql;
using TagBites.Sql.TransactSql;

namespace TagBites.DB.SqlServer;
public class SqlServerLinkAdapter : DbLinkAdapter
{
    public override SqlQueryResolver QueryResolver { get; } = new TransactSqlQueryResolver();
    public override int DefaultPort => 0;


    protected override DbConnection CreateConnection(string connectionString)
    {
        var connection = new SqlConnection(connectionString);
        return connection;
    }
    protected override string CreateConnectionString(DbConnectionArguments arguments)
    {
        var sb = new SqlConnectionStringBuilder();

        foreach (var key in arguments.Keys)
            if (sb.ContainsKey(key))
                sb[key] = arguments[key];

        if (string.IsNullOrEmpty(sb.DataSource))
        {
            var port = arguments.Port;
            sb.DataSource = port > 0 ? arguments.Host + ',' + DefaultPort : arguments.Host;
        }

        sb.InitialCatalog = arguments["initial catalog"] ?? arguments.Database;

        if (!string.IsNullOrWhiteSpace(arguments.Username))
            sb.UserID = arguments.Username;

        if (!string.IsNullOrWhiteSpace(arguments.Password))
            sb.Password = arguments.Password;

        sb.Enlist = false;
        sb.Pooling = false;

        return sb.ToString();
    }

    protected override DbCommand CreateCommand(Query query)
    {
        var cmd = new SqlCommand(query.Command);

        foreach (var item in query.Parameters)
        {
            var value = item.Value;
            cmd.Parameters.Add(new SqlParameter(item.Name, value));
        }

        return cmd;
    }
    protected override DbDataAdapter CreateDataAdapter(DbCommand command)
    {
        return new SqlDataAdapter((SqlCommand)command);
    }
}
