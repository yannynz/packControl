namespace PackControl.Application.Settings;

public sealed record AccessUserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    bool MfaRequired,
    bool Active);

public sealed record EstimatorParameterDto(
    string Label,
    string Value,
    string Unit);

public sealed record CompanyProfileDto(
    string TradeName,
    string DocumentNumber,
    string StateRegistration,
    string FiscalSeries,
    bool NfeEnabled,
    string Environment,
    string AdapterName,
    string CertificateType,
    string CertificateMedia);

public sealed record IntegrationStatusDto(
    string Name,
    string Status,
    string Notes);

public sealed record SettingsOverviewDto(
    IReadOnlyList<AccessUserDto> Users,
    IReadOnlyList<EstimatorParameterDto> EstimatorParameters,
    IReadOnlyList<CompanyProfileDto> Companies,
    IReadOnlyList<IntegrationStatusDto> Integrations);
