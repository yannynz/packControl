namespace PackControl.Application.Registers;

public sealed record CreateRegisterEntryRequest(
    string GroupKey,
    string Name,
    string? Description);
