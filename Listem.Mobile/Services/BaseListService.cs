using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public class BaseListService(IServiceProvider serviceProvider) : IOnlineListService
{
    private readonly IOnlineListService _onlineService =
        serviceProvider.GetService<IOnlineListService>()!;
    private readonly IOfflineListService _offlineService =
        serviceProvider.GetService<IOfflineListService>()!;
    private readonly AuthService _authService = serviceProvider.GetService<AuthService>()!;

    public async Task<List<ObservableList>> GetAllAsync()
    {
        if (_authService.IsUserSignedIn())
        {
            return await _onlineService.GetAllAsync();
        }
        return await _offlineService.GetAllAsync();
    }

    public async Task CreateOrUpdateAsync(ObservableList observableList)
    {
        if (_authService.IsUserSignedIn())
        {
            await _onlineService.CreateOrUpdateAsync(observableList);
        }
        else
        {
            await _offlineService.CreateOrUpdateAsync(observableList);
        }
    }

    public async Task DeleteAsync(ObservableList observableList)
    {
        if (_authService.IsUserSignedIn())
        {
            await _onlineService.DeleteAsync(observableList);
        }
        else
        {
            await _offlineService.DeleteAsync(observableList);
        }
    }

    public async Task DeleteAllAsync()
    {
        if (_authService.IsUserSignedIn())
        {
            await _onlineService.DeleteAllAsync();
        }
        else
        {
            await _offlineService.DeleteAllAsync();
        }
    }
}
