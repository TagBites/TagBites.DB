using System.Text.Json;

namespace TagBites;

public class ConnectionSettings
{
    private const string FileName = "connection.json";
    private const string ExampleFileName = "connection.example.json";

    private static readonly JsonSerializerOptions s_serializerOptions = new() { PropertyNameCaseInsensitive = true };
    private static ConnectionSettings s_current;

    public static ConnectionSettings Current => s_current ??= Load();

    public PostgresConnectionSettings Postgres { get; set; }
    public string SqlServerConnectionString { get; set; }
    public string SqliteDatabase { get; set; }


    private static ConnectionSettings Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, FileName);
        if (!File.Exists(path))
            throw new InvalidOperationException($"Test connection settings are missing. Copy '{ExampleFileName}' to '{FileName}' in the test project and fill in the values.");

        var settings = JsonSerializer.Deserialize<ConnectionSettings>(File.ReadAllText(path), s_serializerOptions);
        if (settings == null)
            throw new InvalidOperationException($"'{FileName}' contains no settings.");

        return settings;
    }


    public class PostgresConnectionSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
