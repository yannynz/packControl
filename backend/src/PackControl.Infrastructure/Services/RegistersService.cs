using System.Text.Json;
using PackControl.Application.Abstractions;
using PackControl.Application.Registers;
using PackControl.Domain.Audit;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class RegistersService(
    AppStateStore stateStore,
    IClock clock,
    ICurrentUserAccessor currentUserAccessor,
    IAppStatePersistence statePersistence) : IRegistersService
{
    private static readonly IReadOnlyList<(string Key, string Label)> GroupDefinitions =
    [
        ("tipos_faca", "Tipos de faca"),
        ("tipos_destacador", "Tipos de destacador"),
        ("tipos_borracha", "Tipos de borracha"),
        ("tipos_material", "Tipos de material"),
        ("setores", "Setores"),
        ("operacoes", "Operacoes"),
        ("modos_entrega", "Modos de entrega"),
        ("fornecedores", "Fornecedores"),
        ("unidades_medida", "Unidades de medida")
    ];

    public async Task<RegistersOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return MapOverview(stateStore.RegisterEntries);
        }
    }

    public async Task<RegistersOverviewDto> CreateAsync(CreateRegisterEntryRequest request, CancellationToken cancellationToken)
    {
        RegistersOverviewDto overview;
        lock (stateStore.SyncRoot)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("O cadastro precisa de um nome.");
            }

            var group = ResolveGroup(request.GroupKey);
            if (group is null)
            {
                throw new InvalidOperationException("Grupo de cadastro invalido.");
            }

            var entry = new RegisterEntryState
            {
                Id = Guid.NewGuid(),
                GroupKey = group.Value.Key,
                GroupLabel = group.Value.Label,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                Active = true,
                UpdatedAtUtc = clock.UtcNow
            };

            stateStore.RegisterEntries.Add(entry);
            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                "Register",
                entry.Id,
                "register.created",
                $"Cadastro {entry.Name} criado em {entry.GroupLabel}.",
                JsonSerializer.Serialize(new { entry.GroupKey, entry.Description }),
                clock.UtcNow));

            overview = MapOverview(stateStore.RegisterEntries);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return overview;
    }

    public async Task<RegistersOverviewDto> UpdateAsync(Guid registerEntryId, UpdateRegisterEntryRequest request, CancellationToken cancellationToken)
    {
        RegistersOverviewDto overview;
        lock (stateStore.SyncRoot)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("O cadastro precisa de um nome.");
            }

            var entry = stateStore.RegisterEntries.SingleOrDefault(x => x.Id == registerEntryId);
            if (entry is null)
            {
                return MapOverview(stateStore.RegisterEntries);
            }

            entry.Name = request.Name.Trim();
            entry.Description = request.Description?.Trim() ?? string.Empty;
            entry.Active = request.Active;
            entry.UpdatedAtUtc = clock.UtcNow;

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                "Register",
                entry.Id,
                "register.updated",
                $"Cadastro {entry.Name} atualizado em {entry.GroupLabel}.",
                JsonSerializer.Serialize(new { entry.Active, entry.Description }),
                clock.UtcNow));

            overview = MapOverview(stateStore.RegisterEntries);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return overview;
    }

    private static RegistersOverviewDto MapOverview(IEnumerable<RegisterEntryState> entries)
    {
        var groupedEntries = entries.ToLookup(x => x.GroupKey);

        var groups = GroupDefinitions
            .Select(group => new RegisterGroupDto(
                group.Key,
                group.Label,
                groupedEntries[group.Key]
                    .OrderBy(x => x.Name)
                    .Select(x => new RegisterEntryDto(
                        x.Id,
                        x.GroupKey,
                        x.GroupLabel,
                        x.Name,
                        x.Description,
                        x.Active,
                        x.UpdatedAtUtc))
                    .ToList()))
            .ToList();

        return new RegistersOverviewDto(groups);
    }

    private static (string Key, string Label)? ResolveGroup(string groupKey)
    {
        var normalizedGroupKey = groupKey.Trim().ToLowerInvariant();
        foreach (var group in GroupDefinitions)
        {
            if (group.Key == normalizedGroupKey)
            {
                return group;
            }
        }

        return null;
    }
}
