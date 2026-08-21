namespace PackControl.Application.Auth;

public interface IAuthService
{
    Task<AuthUserDto?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthUserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
}
