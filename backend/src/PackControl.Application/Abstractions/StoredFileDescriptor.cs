namespace PackControl.Application.Abstractions;

public sealed record StoredFileDescriptor(
    string OriginalFileName,
    string StoredFileName,
    string StoragePath,
    string ContentType,
    long SizeBytes,
    string Sha256);
