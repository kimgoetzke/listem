using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using MongoDB.Bson;
using Realms;

namespace Listem.Mobile.Services;

public class ItemService : IItemService
{
    private readonly Realm _realm = RealmService.GetMainThreadRealm();

    public async Task CreateAsync(Item item)
    {
        await _realm.WriteAsync(() =>
        {
            item.IsDraft = false;
            _realm.Add(item);
            Logger.Log($"Added: {item.ToLoggableString()}");
        });
    }

    public async Task UpdateAsync(
        Item item,
        string? name = null,
        string? ownedBy = null,
        ISet<string>? sharedWith = null,
        Category? category = null,
        int? quantity = null,
        bool? isImportant = null
    )
    {
        await _realm.WriteAsync(() =>
        {
            if (_realm.Find<Item>(item.Id) == null)
            {
                Logger.Log($"Not updated because it doesn't exist: {item.ToLoggableString()}");
                return;
            }
            if (name != null)
            {
                item.Name = name;
            }
            if (ownedBy != null)
            {
                item.OwnedBy = ownedBy;
            }
            if (sharedWith != null)
            {
                item.SharedWith.Clear();
                foreach (var user in sharedWith)
                {
                    item.SharedWith.Add(user);
                }
            }
            if (category != null)
            {
                item.Category = new Category { Name = category.Name };
            }
            if (quantity != null)
            {
                item.Quantity = (int)quantity;
            }
            if (isImportant != null)
            {
                item.IsImportant = (bool)isImportant;
            }
            item.UpdatedOn = DateTimeOffset.Now.ToUniversalTime();
            Logger.Log($"Updated: {item.ToLoggableString()}");
        });
    }

    public async Task DeleteAsync(Item item)
    {
        Logger.Log($"Removing item: {item.Name} {item.Id}");
        await _realm.WriteAsync(() =>
        {
            _realm.Remove(item);
        });
    }

    public async Task DeleteAllInListAsync(List list)
    {
        Logger.Log($"Removed all items in list '{list.Name}' (not implemented yet)");
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
