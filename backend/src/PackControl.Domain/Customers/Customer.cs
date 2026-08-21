using PackControl.Domain.Common;

namespace PackControl.Domain.Customers;

public sealed class Customer : AuditableEntity
{
    private Customer()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string? DocumentNumber { get; private set; }
    public string? ContactName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Notes { get; private set; }
    public List<string> Nicknames { get; private set; } = [];
    public string? PostalCode { get; private set; }
    public string? Street { get; private set; }
    public string? StreetNumber { get; private set; }
    public string? District { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? CityIbgeCode { get; private set; }
    public string? StateRegistration { get; private set; }
    public string TaxpayerIndicator { get; private set; } = "NaoContribuinte";
    public string? Complement { get; private set; }
    public string? ReferencePoint { get; private set; }
    public Guid? DefaultCarrierId { get; private set; }
    public string? DefaultCarrierName { get; private set; }
    public string? DefaultDeliveryMode { get; private set; }
    public List<CustomerProductPricingRule> ProductPricingRules { get; private set; } = [];
    public int Score { get; private set; }

    public static Customer Create(
        string name,
        string? documentNumber,
        string? contactName,
        string? email,
        string? phone,
        string? notes,
        IEnumerable<string>? nicknames,
        string? postalCode,
        string? street,
        string? streetNumber,
        string? district,
        string? city,
        string? state,
        string? cityIbgeCode,
        string? stateRegistration,
        string? taxpayerIndicator,
        string? complement,
        string? referencePoint,
        Guid? defaultCarrierId,
        string? defaultCarrierName,
        string? defaultDeliveryMode,
        IEnumerable<CustomerProductPricingRule>? productPricingRules,
        int score,
        DateTime utcNow,
        string actor)
    {
        var customer = new Customer
        {
            Name = name.Trim(),
            DocumentNumber = documentNumber?.Trim(),
            ContactName = contactName?.Trim(),
            Email = email?.Trim(),
            Phone = phone?.Trim(),
            Notes = notes?.Trim(),
            Nicknames = NormalizeNicknames(nicknames),
            PostalCode = Normalize(postalCode),
            Street = Normalize(street),
            StreetNumber = Normalize(streetNumber),
            District = Normalize(district),
            City = Normalize(city),
            State = Normalize(state),
            CityIbgeCode = Normalize(cityIbgeCode),
            StateRegistration = Normalize(stateRegistration),
            TaxpayerIndicator = NormalizeTaxpayerIndicator(taxpayerIndicator),
            Complement = Normalize(complement),
            ReferencePoint = Normalize(referencePoint),
            DefaultCarrierId = defaultCarrierId,
            DefaultCarrierName = Normalize(defaultCarrierName),
            DefaultDeliveryMode = Normalize(defaultDeliveryMode),
            ProductPricingRules = NormalizePricingRules(productPricingRules),
            Score = Math.Clamp(score, 0, 100)
        };

        customer.MarkCreated(utcNow, actor);
        return customer;
    }

    public static Customer Restore(
        Guid id,
        string name,
        string? documentNumber,
        string? contactName,
        string? email,
        string? phone,
        string? notes,
        IEnumerable<string>? nicknames,
        string? postalCode,
        string? street,
        string? streetNumber,
        string? district,
        string? city,
        string? state,
        string? cityIbgeCode,
        string? stateRegistration,
        string taxpayerIndicator,
        string? complement,
        string? referencePoint,
        Guid? defaultCarrierId,
        string? defaultCarrierName,
        string? defaultDeliveryMode,
        IEnumerable<CustomerProductPricingRule>? productPricingRules,
        int score,
        DateTime createdAtUtc,
        string createdBy,
        DateTime? updatedAtUtc,
        string? updatedBy)
    {
        return new Customer
        {
            Id = id,
            Name = name,
            DocumentNumber = documentNumber,
            ContactName = contactName,
            Email = email,
            Phone = phone,
            Notes = notes,
            Nicknames = nicknames?.ToList() ?? [],
            PostalCode = postalCode,
            Street = street,
            StreetNumber = streetNumber,
            District = district,
            City = city,
            State = state,
            CityIbgeCode = cityIbgeCode,
            StateRegistration = stateRegistration,
            TaxpayerIndicator = NormalizeTaxpayerIndicator(taxpayerIndicator),
            Complement = complement,
            ReferencePoint = referencePoint,
            DefaultCarrierId = defaultCarrierId,
            DefaultCarrierName = defaultCarrierName,
            DefaultDeliveryMode = defaultDeliveryMode,
            ProductPricingRules = productPricingRules?.ToList() ?? [],
            Score = score,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            UpdatedAtUtc = updatedAtUtc,
            UpdatedBy = updatedBy
        };
    }

    public void Update(
        string name,
        string? documentNumber,
        string? contactName,
        string? email,
        string? phone,
        string? notes,
        IEnumerable<string>? nicknames,
        string? postalCode,
        string? street,
        string? streetNumber,
        string? district,
        string? city,
        string? state,
        string? cityIbgeCode,
        string? stateRegistration,
        string? taxpayerIndicator,
        string? complement,
        string? referencePoint,
        Guid? defaultCarrierId,
        string? defaultCarrierName,
        string? defaultDeliveryMode,
        IEnumerable<CustomerProductPricingRule>? productPricingRules,
        int score,
        DateTime utcNow,
        string actor)
    {
        Name = name.Trim();
        DocumentNumber = Normalize(documentNumber);
        ContactName = Normalize(contactName);
        Email = Normalize(email);
        Phone = Normalize(phone);
        Notes = Normalize(notes);
        Nicknames = NormalizeNicknames(nicknames);
        PostalCode = Normalize(postalCode);
        Street = Normalize(street);
        StreetNumber = Normalize(streetNumber);
        District = Normalize(district);
        City = Normalize(city);
        State = Normalize(state);
        CityIbgeCode = Normalize(cityIbgeCode);
        StateRegistration = Normalize(stateRegistration);
        TaxpayerIndicator = NormalizeTaxpayerIndicator(taxpayerIndicator);
        Complement = Normalize(complement);
        ReferencePoint = Normalize(referencePoint);
        DefaultCarrierId = defaultCarrierId;
        DefaultCarrierName = Normalize(defaultCarrierName);
        DefaultDeliveryMode = Normalize(defaultDeliveryMode);
        ProductPricingRules = NormalizePricingRules(productPricingRules);
        Score = Math.Clamp(score, 0, 100);
        MarkUpdated(utcNow, actor);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeTaxpayerIndicator(string? value)
    {
        var normalized = Normalize(value);
        return normalized switch
        {
            "Contribuinte" => "Contribuinte",
            "Isento" => "Isento",
            _ => "NaoContribuinte"
        };
    }

    private static List<string> NormalizeNicknames(IEnumerable<string>? nicknames) =>
        (nicknames ?? [])
            .Select(Normalize)
            .OfType<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static List<CustomerProductPricingRule> NormalizePricingRules(IEnumerable<CustomerProductPricingRule>? rules) =>
        (rules ?? [])
            .Where(rule => rule.ProductTemplateId != Guid.Empty)
            .GroupBy(rule => rule.ProductTemplateId)
            .Select(group => group.Last())
            .ToList();
}
