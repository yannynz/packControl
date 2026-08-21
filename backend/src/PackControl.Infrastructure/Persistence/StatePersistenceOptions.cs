namespace PackControl.Infrastructure.Persistence;

public sealed class StatePersistenceOptions
{
    public string Provider { get; set; } = "InMemory";
    public string? ConnectionString { get; set; }
    public string Schema { get; set; } = "public";
    public string SnapshotKey { get; set; } = "main";
}
