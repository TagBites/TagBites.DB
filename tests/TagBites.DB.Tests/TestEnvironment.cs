using System.Reflection;

namespace TagBites;

public static class TestEnvironment
{
    public const string ProviderVariableName = "TAGBITES_DB_PROVIDER";
    public const string CompatibleNpgsqlConfiguration = "Debug-CompatibleNpgsql";

    private const string PostgresProvider = "postgres";
    private const string SqliteProvider = "sqlite";

    public static string Provider { get; } = ReadProvider();

    public static bool IsCompatibleNpgsql { get; } = typeof(TestEnvironment).Assembly
        .GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration == CompatibleNpgsqlConfiguration;

    public static bool IsPostgres => Provider == PostgresProvider;
    public static bool IsSqlite => Provider == SqliteProvider;


    private static string ReadProvider()
    {
        var value = Environment.GetEnvironmentVariable(ProviderVariableName);
        if (string.IsNullOrWhiteSpace(value))
            return PostgresProvider;

        value = value.Trim().ToLowerInvariant();
        if (value != PostgresProvider && value != SqliteProvider)
            throw new InvalidOperationException($"{ProviderVariableName} must be '{PostgresProvider}' or '{SqliteProvider}', not '{value}'.");

        return value;
    }
}
