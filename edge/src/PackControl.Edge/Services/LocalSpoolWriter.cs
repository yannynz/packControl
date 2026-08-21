using System.Text.Json;
using PackControl.Contracts.Edge;
using PackControl.Edge.Configuration;

namespace PackControl.Edge.Services;

public sealed class LocalSpoolWriter(
    IHostEnvironment hostEnvironment,
    ILogger<LocalSpoolWriter> logger,
    IConfiguration configuration)
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task AppendAsync(EdgeEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var options = configuration.GetSection("Edge").Get<EdgeOptions>() ?? new EdgeOptions();
        var spoolRoot = Path.IsPathRooted(options.SpoolDirectory)
            ? options.SpoolDirectory
            : Path.GetFullPath(Path.Combine(hostEnvironment.ContentRootPath, "..", "..", "..", options.SpoolDirectory));

        Directory.CreateDirectory(spoolRoot);
        var spoolFile = Path.Combine(spoolRoot, $"edge-events-{DateTime.UtcNow:yyyyMMdd}.ndjson");
        var serialized = JsonSerializer.Serialize(envelope);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(spoolFile, serialized + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }

        logger.LogInformation("Evento {EventType} armazenado em spool local: {File}", envelope.EventType, spoolFile);
    }
}
