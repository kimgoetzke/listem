using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using MongoDB.Bson;
using Realms;

namespace Listem.Mobile.Services;

public class ItemService : IItemService
{
    private readonly Realm _realm = RealmService.GetMainThreadRealm();

    public async Task<List<Item>> GetAllByListIdAsync(ObjectId listId)
    {
        Logger.Log($"Getting all items for list {listId} (not implemented yet)");
        return [];
    }

    public async Task CreateOrUpdateAsync(Item item)
    {
        Logger.Log($"Added or updated item: {item.ToLoggableString()} (not implemented yet)");
    }

    public async Task DeleteAsync(Item item)
    {
        Logger.Log($"Removing item: {item.Name} {item.Id}");
        await _realm.WriteAsync(() =>
        {
            _realm.Remove(item);
        });
    }

    public async Task DeleteAllByListIdAsync(ObjectId listId)
    {
        Logger.Log($"Removed all items from list {listId} (not implemented yet)");
    }

    public async Task UpdateAllToDefaultCategoryAsync(ObjectId listId)
    {
        Logger.Log($"Updated all items to use default category (not implemented yet)");
    }

    public async Task UpdateAllToCategoryAsync(string categoryName, ObjectId listId)
    {
        Logger.Log($"Updated all items to use category {categoryName} (not implemented yet)");
    }
}
