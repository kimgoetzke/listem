using Listem.Mobile.Models;
using Listem.Mobile.Utilities;

namespace Listem.Mobile.Services;

public class ItemService : ILocalItemService
{
    public async Task<List<ObservableItem>> GetAllByListIdAsync(string listId)
    {
        Logger.Log($"Getting all items for list {listId}");
        return [];
    }

    public async Task CreateOrUpdateAsync(ObservableItem observableItem)
    {
        Logger.Log($"Added or updated item: {observableItem.ToLoggableString()}");
    }

    public async Task DeleteAsync(ObservableItem observableItem)
    {
        Logger.Log($"Removing item: {observableItem.Title} {observableItem.Id}");
    }

    public async Task DeleteAllByListIdAsync(string listId)
    {
        Logger.Log($"Removed all items from list {listId}");
    }

    public async Task UpdateAllToDefaultCategoryAsync(string listId)
    {
        Logger.Log($"Updated all items to use default category");
    }

    public async Task UpdateAllToCategoryAsync(string categoryName, string listId)
    {
        Logger.Log($"Updated all items to use category {categoryName}");
    }
}
