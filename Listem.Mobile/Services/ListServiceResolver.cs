using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public class ListServiceResolver(IServiceProvider sp) : IApiListService
{
    private readonly IApiListService _apiService = sp.GetService<IApiListService>()!;
    private readonly ILocalListService _localService = sp.GetService<ILocalListService>()!;
    private readonly AuthService _authService = sp.GetService<AuthService>()!;

    public async Task<List<ObservableList>> GetAllAsync()
    {
        return _authService.IsUserSignedIn()
            ? await _apiService.GetAllAsync()
            : await _localService.GetAllAsync();
    }

    public async Task CreateOrUpdateAsync(ObservableList observableList)
    {
        if (_authService.IsUserSignedIn())
        {
            await _apiService.CreateOrUpdateAsync(observableList);
        }
        else
        {
            await _localService.CreateOrUpdateAsync(observableList);
        }
    }

    public async Task DeleteAsync(ObservableList observableList)
    {
        if (_authService.IsUserSignedIn())
        {
            await _apiService.DeleteAsync(observableList);
        }
        else
        {
            await _localService.DeleteAsync(observableList);
        }
    }

    public async Task DeleteAllAsync()
    {
        if (_authService.IsUserSignedIn())
        {
            await _apiService.DeleteAllAsync();
        }
        else
        {
            await _localService.DeleteAllAsync();
        }
    }
}
