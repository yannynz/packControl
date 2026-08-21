namespace PackControl.Application.Finance;

public sealed record CreateFinanceEntryRequest(
    Guid? OrderId,
    string? OrderNumber,
    string Type,
    string Description,
    string Counterparty,
    decimal Amount,
    DateTime DueAtUtc,
    string PaymentMethod,
    string? Notes,
    string EntrySource);
