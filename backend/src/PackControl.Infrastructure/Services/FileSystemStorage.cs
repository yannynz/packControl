using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PackControl.Application.Abstractions;

namespace PackControl.Infrastructure.Services;

public sealed class FileSystemStorage(
    IOptions<FileSystemStorageOptions> options,
    IHostEnvironment hostEnvironment) : IFileStorage
{
    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        var fullPath = ResolveFullPath(storagePath);
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public async Task<StoredFileDescriptor> SaveAsync(
        Stream source,
        string originalFileName,
        string? contentType,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var relativeFolder = Path.Combine(now.ToString("yyyy"), now.ToString("MM"));
        var storageRoot = ResolveStorageRoot();
        var destinationFolder = Path.Combine(storageRoot, relativeFolder);
        Directory.CreateDirectory(destinationFolder);

        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(destinationFolder, storedFileName);

        await using (var destination = File.Create(fullPath))
        {
            await source.CopyToAsync(destination, cancellationToken);
        }

        await using var hashStream = File.OpenRead(fullPath);
        var hashBytes = await SHA256.HashDataAsync(hashStream, cancellationToken);
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        var fileInfo = new FileInfo(fullPath);

        return new StoredFileDescriptor(
            originalFileName,
            storedFileName,
            Path.Combine(relativeFolder, storedFileName).Replace('\\', '/'),
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            fileInfo.Length,
            hash);
    }

    private string ResolveFullPath(string storagePath)
    {
        var normalized = storagePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(ResolveStorageRoot(), normalized);
    }

    private string ResolveStorageRoot()
    {
        var rootPath = options.Value.RootPath;
        return Path.IsPathRooted(rootPath)
            ? rootPath
            : Path.GetFullPath(Path.Combine(hostEnvironment.ContentRootPath, "..", "..", "..", rootPath));
    }
}
