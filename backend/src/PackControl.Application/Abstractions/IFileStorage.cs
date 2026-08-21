namespace PackControl.Application.Abstractions;

public interface IFileStorage
{
    Task<StoredFileDescriptor> SaveAsync(
        Stream source,
        string originalFileName,
        string? contentType,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken);
}
