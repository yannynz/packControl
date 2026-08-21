using PackControl.Application.Abstractions;

namespace PackControl.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
