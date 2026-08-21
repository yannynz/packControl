using PackControl.Domain.Common;

namespace PackControl.Domain.Orders;

public sealed class OrderScopeItem : AuditableEntity
{
    private OrderScopeItem()
    {
    }

    public Guid OrderId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Guid? ProductTemplateId { get; private set; }
    public string? ProductName { get; private set; }
    public string? BillingMethod { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public string? Notes { get; private set; }

    internal static OrderScopeItem Create(
        Guid orderId,
        string title,
        string category,
        int quantity,
        Guid? productTemplateId,
        string? productName,
        string? billingMethod,
        decimal? unitPrice,
        string? notes,
        DateTime utcNow,
        string actor)
    {
        var item = new OrderScopeItem
        {
            OrderId = orderId,
            Title = title.Trim(),
            Category = category.Trim(),
            Quantity = Math.Max(1, quantity),
            ProductTemplateId = productTemplateId,
            ProductName = Normalize(productName),
            BillingMethod = Normalize(billingMethod),
            UnitPrice = unitPrice is > 0 ? decimal.Round(unitPrice.Value, 2) : null,
            Notes = notes?.Trim()
        };

        item.MarkCreated(utcNow, actor);
        return item;
    }

    public static OrderScopeItem Restore(
        Guid id,
        Guid orderId,
        string title,
        string category,
        int quantity,
        Guid? productTemplateId,
        string? productName,
        string? billingMethod,
        decimal? unitPrice,
        string? notes,
        DateTime createdAtUtc,
        string createdBy,
        DateTime? updatedAtUtc,
        string? updatedBy)
    {
        return new OrderScopeItem
        {
            Id = id,
            OrderId = orderId,
            Title = title,
            Category = category,
            Quantity = quantity,
            ProductTemplateId = productTemplateId,
            ProductName = productName,
            BillingMethod = billingMethod,
            UnitPrice = unitPrice,
            Notes = notes,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            UpdatedAtUtc = updatedAtUtc,
            UpdatedBy = updatedBy
        };
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
