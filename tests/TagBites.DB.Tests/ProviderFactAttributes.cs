using Xunit.Sdk;

namespace TagBites;

[XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.dotnet")]
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (!TestEnvironment.IsPostgres)
            Skip = $"Needs PostgreSQL, but {TestEnvironment.ProviderVariableName} is '{TestEnvironment.Provider}'.";
    }
}

[XunitTestCaseDiscoverer("Xunit.Sdk.TheoryDiscoverer", "xunit.execution.dotnet")]
public sealed class PostgresTheoryAttribute : TheoryAttribute
{
    public PostgresTheoryAttribute()
    {
        if (!TestEnvironment.IsPostgres)
            Skip = $"Needs PostgreSQL, but {TestEnvironment.ProviderVariableName} is '{TestEnvironment.Provider}'.";
    }
}

[XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.dotnet")]
public sealed class CompatibleNpgsqlFactAttribute : FactAttribute
{
    public CompatibleNpgsqlFactAttribute()
    {
        if (!TestEnvironment.IsCompatibleNpgsql)
            Skip = $"Needs the {TestEnvironment.CompatibleNpgsqlConfiguration} configuration, which swaps in the Vendo build of Npgsql.";
    }
}

[XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.dotnet")]
public sealed class StockNpgsqlFactAttribute : FactAttribute
{
    public StockNpgsqlFactAttribute()
    {
        if (TestEnvironment.IsCompatibleNpgsql)
            Skip = $"Needs the released Npgsql package, which the {TestEnvironment.CompatibleNpgsqlConfiguration} configuration replaces.";
    }
}

[XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.dotnet")]
public sealed class SqliteFactAttribute : FactAttribute
{
    public SqliteFactAttribute()
    {
        if (!TestEnvironment.IsSqlite)
            Skip = $"Needs SQLite, but {TestEnvironment.ProviderVariableName} is '{TestEnvironment.Provider}'.";
    }
}
