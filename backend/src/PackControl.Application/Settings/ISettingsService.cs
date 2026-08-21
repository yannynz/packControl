namespace PackControl.Application.Settings;

public interface ISettingsService
{
    Task<SettingsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
}
