namespace PackControl.Domain.Customers;

public sealed class CustomerProductPricingRule
{
    private CustomerProductPricingRule()
    {
    }

    public Guid ProductTemplateId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string BillingMethod { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public string? Notes { get; private set; }

    public static CustomerProductPricingRule Create(
        Guid productTemplateId,
        string productName,
        string billingMethod,
        decimal unitPrice,
        string? notes) => new()
        {
            ProductTemplateId = productTemplateId,
            ProductName = productName.Trim(),
            BillingMethod = billingMethod.Trim(),
            UnitPrice = decimal.Round(Math.Max(0, unitPrice), 2),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

    public static CustomerProductPricingRule Restore(
        Guid productTemplateId,
        string productName,
        string billingMethod,
        decimal unitPrice,
        string? notes) => new()
        {
            ProductTemplateId = productTemplateId,
            ProductName = productName,
            BillingMethod = billingMethod,
            UnitPrice = unitPrice,
            Notes = notes
        };
}
