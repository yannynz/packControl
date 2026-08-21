using PackControl.Domain.Common;

namespace PackControl.Domain.Orders;

public sealed class OrderAttachment : AuditableEntity
{
    private OrderAttachment()
    {
    }

    public Guid OrderId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string Sha256 { get; private set; } = string.Empty;

    internal static OrderAttachment Create(
        Guid orderId,
        string originalFileName,
        string storedFileName,
        string storagePath,
        string contentType,
        long sizeBytes,
        string sha256,
        DateTime utcNow,
        string actor)
    {
        var attachment = new OrderAttachment
        {
            OrderId = orderId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            StoragePath = storagePath,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Sha256 = sha256
        };

        attachment.MarkCreated(utcNow, actor);
        return attachment;
    }

    public static OrderAttachment Restore(
        Guid id,
        Guid orderId,
        string originalFileName,
        string storedFileName,
        string storagePath,
        string contentType,
        long sizeBytes,
        string sha256,
        DateTime createdAtUtc,
        string createdBy,
        DateTime? updatedAtUtc,
        string? updatedBy)
    {
        return new OrderAttachment
        {
            Id = id,
            OrderId = orderId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            StoragePath = storagePath,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Sha256 = sha256,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            UpdatedAtUtc = updatedAtUtc,
            UpdatedBy = updatedBy
        };
    }
}
