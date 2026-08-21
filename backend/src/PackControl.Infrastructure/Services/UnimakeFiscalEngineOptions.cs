namespace PackControl.Infrastructure.Services;

public sealed class UnimakeFiscalEngineOptions
{
    public const string SectionName = "FiscalEngines:Unimake";

    public string SchemaVersion { get; set; } = "4.00";
    public string DefaultStateCode { get; set; } = "SP";
    public bool UseCertificateForStatusService { get; set; }
    public bool AllowRealEmission { get; set; }
    public string ProcessVersion { get; set; } = "PackControl 2026.03";
    public int ReceiptPollMaxAttempts { get; set; } = 5;
    public int ReceiptPollDelayMs { get; set; } = 1500;
    public string? CertificatePath { get; set; }
    public string? CertificateBase64 { get; set; }
    public string? CertificatePassword { get; set; }
    public string? CertificateThumbprint { get; set; }
    public string? HostHomologacao { get; set; }
    public string? HostProducao { get; set; }
    public string? RequestUriHomologacao { get; set; }
    public string? RequestUriProducao { get; set; }
    public string? WebEnderecoHomologacao { get; set; }
    public string? WebEnderecoProducao { get; set; }
}
