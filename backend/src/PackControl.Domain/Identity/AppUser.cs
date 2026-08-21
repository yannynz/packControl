using PackControl.Domain.Common;

namespace PackControl.Domain.Identity;

public sealed class AppUser : AuditableEntity
{
    private AppUser()
    {
    }

    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    public static AppUser Create(
        string email,
        string fullName,
        UserRole role,
        string passwordHash,
        DateTime utcNow,
        string actor)
    {
        var user = new AppUser
        {
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            Role = role,
            PasswordHash = passwordHash,
            IsActive = true
        };

        user.MarkCreated(utcNow, actor);
        return user;
    }

    public static AppUser Restore(
        Guid id,
        string email,
        string fullName,
        string passwordHash,
        UserRole role,
        bool isActive,
        DateTime createdAtUtc,
        string createdBy,
        DateTime? updatedAtUtc,
        string? updatedBy)
    {
        return new AppUser
        {
            Id = id,
            Email = email,
            FullName = fullName,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = isActive,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            UpdatedAtUtc = updatedAtUtc,
            UpdatedBy = updatedBy
        };
    }

    public void SetPasswordHash(string passwordHash, DateTime utcNow, string actor)
    {
        PasswordHash = passwordHash;
        MarkUpdated(utcNow, actor);
    }
}
