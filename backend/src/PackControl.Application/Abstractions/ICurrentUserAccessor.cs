using PackControl.Domain.Identity;

namespace PackControl.Application.Abstractions;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
    string DisplayName { get; }
    UserRole? Role { get; }
}
