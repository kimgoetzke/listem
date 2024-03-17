using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public interface IItemService
{
    Task<List<ObservableItem>> GetAllAsync();
    Task<List<ObservableItem>> GetAllByListIdAsync(string listId);
    Task CreateOrUpdateAsync(ObservableItem observableItem);
    Task DeleteAsync(ObservableItem observableItem);
    Task DeleteAllByListIdAsync(string listId);
    Task UpdateAllToDefaultCategoryAsync(string listId);
    Task UpdateAllToCategoryAsync(string categoryName, string listId);
}

public interface IOnlineItemService : IItemService { }

public interface IOfflineItemService : IItemService { }
