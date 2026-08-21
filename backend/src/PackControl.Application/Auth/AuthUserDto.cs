namespace PackControl.Application.Auth;

public sealed record AuthUserDto(Guid Id, string FullName, string Email, string Role);
