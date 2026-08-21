namespace PackControl.Edge.Configuration;

public sealed class EdgeOptions
{
    public string MachineName { get; set; } = Environment.MachineName;
    public string SpoolDirectory { get; set; } = "edge/spool";
    public int DebounceSeconds { get; set; } = 2;
    public List<WatchedDirectoryOptions> WatchedDirectories { get; set; } = [];
}
