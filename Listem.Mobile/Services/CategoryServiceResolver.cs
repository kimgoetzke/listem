using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public class CategoryServiceResolver(IServiceProvider sp) : ICategoryService
{
    private readonly IApiCategoryService _apiService = sp.GetService<IApiCategoryService>()!;
    private readonly ILocalCategoryService _localService = sp.GetService<ILocalCategoryService>()!;
    private readonly AuthService _authService = sp.GetService<AuthService>()!;

    public async Task<List<ObservableCategory>> GetAllByListIdAsync(string listId)
    {
        return _authService.IsUserSignedIn()
            ? await _apiService.GetAllByListIdAsync(listId)
            : await _localService.GetAllByListIdAsync(listId);
    }

    public async Task CreateOrUpdateAsync(ObservableCategory observableCategory)
    {
        if (_authService.IsUserSignedIn())
        {
            await _apiService.CreateOrUpdateAsync(observableCategory);
        }
        else
        {
            await _localService.CreateOrUpdateAsync(observableCategory);
        }
    }

    public async Task DeleteAsync(ObservableCategory observableCategory)
    {
        if (_authService.IsUserSignedIn())
        {
            await _apiService.DeleteAsync(observableCategory);
        }
        else
        {
            await _localService.DeleteAsync(observableCategory);
        }
    }

    public async Task DeleteAllByListIdAsync(string listId)
    {
        if (_authService.IsUserSignedIn())
        {
            await _apiService.DeleteAllByListIdAsync(listId);
        }
        else
        {
            await _localService.DeleteAllByListIdAsync(listId);
        }
    }
}
