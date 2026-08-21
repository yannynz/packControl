using System.Security.Claims;
using PackControl.Application.Abstractions;
using PackControl.Domain.Identity;

namespace PackControl.Api.Infrastructure;

public sealed class HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public Guid? UserId
    {
        get
        {
            var raw = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var userId) ? userId : null;
        }
    }

    public string DisplayName =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name) ?? "system";

    public UserRole? Role
    {
        get
        {
            var raw = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(raw, out var role) ? role : null;
        }
    }
}
