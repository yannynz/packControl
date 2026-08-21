namespace PackControl.Infrastructure.Persistence;

public interface IAppStatePersistence
{
    bool Enabled { get; }
    Task LoadAsync(AppStateStore stateStore, CancellationToken cancellationToken);
    Task SaveAsync(AppStateStore stateStore, CancellationToken cancellationToken);
}
