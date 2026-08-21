namespace PackControl.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
