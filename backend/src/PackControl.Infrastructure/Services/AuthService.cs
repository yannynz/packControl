using PackControl.Application.Auth;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class AuthService(
    AppStateStore stateStore,
    PasswordService passwordService) : IAuthService
{
    public async Task<AuthUserDto?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            var user = stateStore.Users.SingleOrDefault(x => x.Email == normalizedEmail && x.IsActive);

            if (user is null)
            {
                return null;
            }

            return passwordService.Verify(user.PasswordHash, password) ? Map(user) : null;
        }
    }

    public async Task<AuthUserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            var user = stateStore.Users.SingleOrDefault(x => x.Id == userId && x.IsActive);
            return user is null ? null : Map(user);
        }
    }

    private static AuthUserDto Map(PackControl.Domain.Identity.AppUser user) =>
        new(user.Id, user.FullName, user.Email, MapRole(user.Role));

    private static string MapRole(PackControl.Domain.Identity.UserRole role) => role switch
    {
        PackControl.Domain.Identity.UserRole.Administrator => "Administrador",
        PackControl.Domain.Identity.UserRole.Sales => "Comercial",
        PackControl.Domain.Identity.UserRole.Engineering => "Engenharia",
        PackControl.Domain.Identity.UserRole.Production => "Producao",
        PackControl.Domain.Identity.UserRole.Logistics => "Logistica",
        PackControl.Domain.Identity.UserRole.Finance => "Financeiro",
        PackControl.Domain.Identity.UserRole.Management => "Gestao",
        _ => role.ToString()
    };
}
