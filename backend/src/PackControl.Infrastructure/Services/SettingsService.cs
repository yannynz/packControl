using PackControl.Application.Settings;
using PackControl.Domain.Identity;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class SettingsService(AppStateStore stateStore) : ISettingsService
{
    public async Task<SettingsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            var users = stateStore.Users
                .OrderBy(x => x.FullName)
                .Select(x => new AccessUserDto(
                    x.Id,
                    x.FullName,
                    x.Email,
                    MapRole(x.Role),
                    x.Role is UserRole.Administrator,
                    x.IsActive))
                .ToList();

            var estimatorParameters = new List<EstimatorParameterDto>
            {
                new("Setup minimo", "45", "min"),
                new("Margem alvo MVP", "28", "%"),
                new("Buffer de prazo", "1.5", "dias"),
                new("Score minimo DXF", "72", "pts")
            };

            var companies = stateStore.FiscalCompanies
                .OrderBy(x => x.TradeName)
                .Select(x => new CompanyProfileDto(
                    x.TradeName,
                    x.DocumentNumber,
                    x.StateRegistration,
                    x.FiscalSeries,
                    x.NfeEnabled,
                    x.Environment,
                    x.AdapterName,
                    x.CertificateType,
                    x.CertificateMedia))
                .ToList();

            var integrations = new List<IntegrationStatusDto>
            {
                new("PackControl Edge", "Preparado", "Watcher local e spool NDJSON ativos para a baseline."),
                new(
                    "NF-e",
                    stateStore.FiscalCompanies.Any(x => x.NfeEnabled) ? "Core fiscal ativo" : "Desligado",
                    stateStore.FiscalCompanies.Any(x => x.NfeEnabled)
                        ? "Perfil fiscal com certificado A1/A3, roteamento de adapter por emitente e diagnostico do autorizador disponiveis."
                        : "Nenhum emissor fiscal habilitado."),
                new("RabbitMQ", "Planejado", "Reservado para borda e pipeline tecnico pesado.")
            };

            return new SettingsOverviewDto(users, estimatorParameters, companies, integrations);
        }
    }

    private static string MapRole(UserRole role) => role switch
    {
        UserRole.Administrator => "Administrador",
        UserRole.Sales => "Comercial",
        UserRole.Production => "Producao",
        UserRole.Logistics => "Logistica",
        UserRole.Finance => "Financeiro",
        UserRole.Engineering => "Engenharia",
        UserRole.Management => "Gestao",
        _ => role.ToString()
    };
}
