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
        await _realm.WriteAsync(() =>
        {
            _realm.RemoveRange(list.Items);
        });
        Logger.Log($"Removed all items in list '{list.Name}' {list.Id}");
    }

    public async Task ResetAllToDefaultCategoryAsync(List list)
    {
        var count = 0;
        await _realm.WriteAsync(() =>
        {
            var defaultCategory = list.Categories.First(c =>
                c.Name == Constants.DefaultCategoryName
            );
            foreach (var item in list.Items)
            {
                item.Category = new Category { Name = defaultCategory.Name };
                count++;
            }
        });
        Logger.Log(
            $"Updated all items ({count}) in list '{list.Name}' {list.Id} to use default category"
        );
    }

    public async Task ResetSelectedToDefaultCategoryAsync(List list, Category category)
    {
        var count = 0;
        await _realm.WriteAsync(() =>
        {
            var defaultCategory = list.Categories.First(c =>
                c.Name == Constants.DefaultCategoryName
            );
            var relevantItems = _realm
                .All<Item>()
                .Where(item => item.List == list)
                .Filter($"Category.Name == $0", category.Name);
            foreach (var item in relevantItems)
            {
                item.Category = new Category { Name = defaultCategory.Name };
                count++;
            }
        });
        Logger.Log(
            $"Updated {count} item(s) with category '{category.Name}' in list '{list.Name}' {list.Id} to use default category"
        );
    }
}
