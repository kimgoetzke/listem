using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public class ItemServiceResolver(IServiceProvider sp) : IItemService
{
    private readonly IApiItemService _apiService = sp.GetService<IApiItemService>()!;
    private readonly ILocalItemService _localService = sp.GetService<ILocalItemService>()!;
    private readonly AuthService _authService = sp.GetService<AuthService>()!;

    public async Task<List<ObservableItem>> GetAllByListIdAsync(string listId)
    {
        return _authService.IsUserSignedIn()
            ? await _apiService.GetAllByListIdAsync(listId)
            : await _localService.GetAllByListIdAsync(listId);
    }

    public async Task CreateOrUpdateAsync(ObservableItem observableItem)
    {
        if (_authService.IsUserSignedIn())
        {
            await _apiService.CreateOrUpdateAsync(observableItem);
        }
        else
        {
            await _localService.CreateOrUpdateAsync(observableItem);
        }
    }

    public async Task DeleteAsync(ObservableItem observableItem)
    {
        if (_authService.IsUserSignedIn())
        {
            await _apiService.DeleteAsync(observableItem);
        }
        else
        {
            await _localService.DeleteAsync(observableItem);
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

    public Task UpdateAllToDefaultCategoryAsync(string listId)
    {
        return _authService.IsUserSignedIn()
            ? _apiService.UpdateAllToDefaultCategoryAsync(listId)
            : _localService.UpdateAllToDefaultCategoryAsync(listId);
    }

    public Task UpdateAllToCategoryAsync(string categoryName, string listId)
    {
        return _authService.IsUserSignedIn()
            ? _apiService.UpdateAllToCategoryAsync(categoryName, listId)
            : _localService.UpdateAllToCategoryAsync(categoryName, listId);
    }
}
