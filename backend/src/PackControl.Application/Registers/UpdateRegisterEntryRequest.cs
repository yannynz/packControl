namespace PackControl.Application.Registers;

public sealed record UpdateRegisterEntryRequest(
    string Name,
    string? Description,
    bool Active);
