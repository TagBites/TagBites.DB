namespace TagBites.DB.Postgres;

public readonly struct PgSqlNotification
{
    public int ProcessId { get; }
    public string Channel { get; }
    public string Message { get; }

    public PgSqlNotification(int processId, string channel, string message)
    {
        ProcessId = processId;
        Channel = channel;
        Message = message;
    }
}
