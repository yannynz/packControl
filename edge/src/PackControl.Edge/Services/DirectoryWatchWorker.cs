using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using PackControl.Contracts.Edge;
using PackControl.Edge.Configuration;

namespace PackControl.Edge.Services;

public sealed class DirectoryWatchWorker(
    IConfiguration configuration,
    ILogger<DirectoryWatchWorker> logger,
    LocalSpoolWriter spoolWriter) : BackgroundService
{
    private readonly List<FileSystemWatcher> _watchers = [];
    private readonly ConcurrentDictionary<string, DateTime> _recentEvents = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = configuration.GetSection("Edge").Get<EdgeOptions>() ?? new EdgeOptions();

        foreach (var watchedDirectory in options.WatchedDirectories.Where(x => !string.IsNullOrWhiteSpace(x.Path)))
        {
            var absolutePath = Path.GetFullPath(watchedDirectory.Path);
            Directory.CreateDirectory(absolutePath);

            var watcher = new FileSystemWatcher(absolutePath)
            {
                IncludeSubdirectories = watchedDirectory.IncludeSubdirectories,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
            };

            watcher.Created += (_, args) => _ = HandleAsync(args.FullPath, options, stoppingToken);
            watcher.Changed += (_, args) => _ = HandleAsync(args.FullPath, options, stoppingToken);

            _watchers.Add(watcher);
            logger.LogInformation("Watch ativo em {Path}", absolutePath);
        }

        if (_watchers.Count == 0)
        {
            logger.LogWarning("Nenhum diretorio configurado para observacao no PackControl Edge.");
        }

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // encerramento normal do worker
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var watcher in _watchers)
        {
            watcher.Dispose();
        }

        return base.StopAsync(cancellationToken);
    }

    private async Task HandleAsync(string fullPath, EdgeOptions options, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var debounceWindow = TimeSpan.FromSeconds(Math.Max(1, options.DebounceSeconds));

        if (_recentEvents.TryGetValue(fullPath, out var lastSeen) && now - lastSeen < debounceWindow)
        {
            return;
        }

        _recentEvents[fullPath] = now;

        var fileInfo = new FileInfo(fullPath);
        var entityRef = fileInfo.Exists
            ? $"{fileInfo.Name}:{fileInfo.Length}"
            : Path.GetFileName(fullPath);

        var payload = new Dictionary<string, string>
        {
            ["fileName"] = Path.GetFileName(fullPath),
            ["extension"] = Path.GetExtension(fullPath).ToLowerInvariant(),
            ["exists"] = fileInfo.Exists.ToString(),
            ["directory"] = fileInfo.DirectoryName ?? string.Empty
        };

        var envelope = new EdgeEventEnvelope(
            Guid.NewGuid(),
            ResolveEventType(fullPath),
            now,
            options.MachineName,
            fullPath,
            entityRef,
            ComputeHash(fullPath, fileInfo),
            payload);

        await spoolWriter.AppendAsync(envelope, cancellationToken);
    }

    private static string ResolveEventType(string fullPath)
    {
        return Path.GetExtension(fullPath).ToLowerInvariant() switch
        {
            ".pdf" => EdgeEventTypes.PdfImported,
            ".dxf" => EdgeEventTypes.DxfAnalysisRequested,
            _ => EdgeEventTypes.FileDetected
        };
    }

    private static string ComputeHash(string fullPath, FileInfo fileInfo)
    {
        var raw = $"{fullPath}|{fileInfo.Exists}|{fileInfo.Length}|{fileInfo.LastWriteTimeUtc:O}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
