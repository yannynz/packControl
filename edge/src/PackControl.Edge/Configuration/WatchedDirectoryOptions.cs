namespace PackControl.Edge.Configuration;

public sealed class WatchedDirectoryOptions
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IncludeSubdirectories { get; set; }
}
