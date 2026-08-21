namespace PackControl.Application.Registers;

public sealed record RegisterEntryDto(
    Guid Id,
    string GroupKey,
    string GroupLabel,
    string Name,
    string Description,
    bool Active,
    DateTime UpdatedAtUtc);

public sealed record RegisterGroupDto(
    string Key,
    string Label,
    IReadOnlyList<RegisterEntryDto> Entries);

public sealed record RegistersOverviewDto(IReadOnlyList<RegisterGroupDto> Groups);
