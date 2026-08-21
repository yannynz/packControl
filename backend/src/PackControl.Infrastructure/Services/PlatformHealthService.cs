using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class PlatformHealthService(
    IOptions<StatePersistenceOptions> statePersistenceOptions,
    IOptions<FileSystemStorageOptions> fileStorageOptions,
    IHostEnvironment hostEnvironment)
{
    public async Task<PlatformHealthReport> CheckAsync(CancellationToken cancellationToken)
    {
        var checks = new[]
        {
            await CheckPersistenceAsync(cancellationToken),
            await CheckStorageAsync(cancellationToken)
        };

        var status = checks.Any(check => check.Status == "fail")
            ? "fail"
            : "ok";

        return new PlatformHealthReport(
            status,
            "PackControl.Api",
            DateTime.UtcNow,
            checks);
    }

    private async Task<PlatformHealthCheckResult> CheckPersistenceAsync(CancellationToken cancellationToken)
    {
        var options = statePersistenceOptions.Value;
        if (!options.Provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return new PlatformHealthCheckResult(
                "state-persistence",
                "ok",
                "Persistencia em memoria/local ativa. PostgreSQL nao esta habilitado nesta instancia.",
                options.Provider);
        }

        try
        {
            await using var connection = new NpgsqlConnection(options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "select 1;";
            await command.ExecuteScalarAsync(cancellationToken);

            return new PlatformHealthCheckResult(
                "state-persistence",
                "ok",
                "Conexao com PostgreSQL validada.",
                options.Schema);
        }
        catch (Exception ex)
        {
            return new PlatformHealthCheckResult(
                "state-persistence",
                "fail",
                $"Falha ao validar PostgreSQL: {ex.Message}",
                options.Schema);
        }
    }

    private async Task<PlatformHealthCheckResult> CheckStorageAsync(CancellationToken cancellationToken)
    {
        var rootPath = ResolveStorageRoot();

        try
        {
            Directory.CreateDirectory(rootPath);

            var probePath = Path.Combine(rootPath, $".health-{Guid.NewGuid():N}.probe");
            await File.WriteAllTextAsync(probePath, "ok", cancellationToken);
            File.Delete(probePath);

            return new PlatformHealthCheckResult(
                "file-storage",
                "ok",
                "Storage local acessivel para leitura/escrita.",
                rootPath);
        }
        catch (Exception ex)
        {
            return new PlatformHealthCheckResult(
                "file-storage",
                "fail",
                $"Falha ao validar storage local: {ex.Message}",
                rootPath);
        }
    }

    private string ResolveStorageRoot()
    {
        var rootPath = fileStorageOptions.Value.RootPath;
        return Path.IsPathRooted(rootPath)
            ? rootPath
            : Path.GetFullPath(Path.Combine(hostEnvironment.ContentRootPath, "..", "..", "..", rootPath));
    }
}

public sealed record PlatformHealthReport(
    string Status,
    string Service,
    DateTime CheckedAtUtc,
    IReadOnlyCollection<PlatformHealthCheckResult> Checks);

public sealed record PlatformHealthCheckResult(
    string Name,
    string Status,
    string Message,
    string? Target);
