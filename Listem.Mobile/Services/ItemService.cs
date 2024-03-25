using Listem.Mobile.Models;
using Microsoft.Extensions.Logging;
using Realms;

namespace Listem.Mobile.Services;

public class ItemService(ILogger<CategoryService> logger) : IItemService
{
    private readonly Realm _realm = RealmService.GetMainThreadRealm();

    public async Task CreateAsync(Item item)
    {
        await _realm.WriteAsync(() =>
        {
            item.IsDraft = false;
            _realm.Add(item);
            logger.LogInformation("Added: {Item}", item.ToLoggableString());
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
                logger.LogInformation(
                    "Not updated because it doesn't exist: {Item}",
                    item.ToLoggableString()
                );
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
            logger.LogInformation("Updated: {Item}", item.ToLoggableString());
        });
    }

    public async Task DeleteAsync(Item item)
    {
        logger.LogInformation("Removing: {Item}", item.ToLoggableString());
        await _realm.WriteAsync(() => _realm.Remove(item));
    }

    public async Task DeleteAllInListAsync(List list)
    {
        await _realm.WriteAsync(() =>
        {
            _realm.RemoveRange(list.Items);
        });
        logger.LogInformation("Removed all items in list '{Name}' {Id}", list.Name, list.Id);
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
        logger.LogInformation(
            "Updated all {Count} item(s) in list '{Name}' {Id} to use default category",
            count,
            list.Name,
            list.Id
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
        logger.LogInformation(
            "Updated {Count} item(s) with category '{Category}' in list '{Name}' {Id} to use default category",
            count,
            category.Name,
            list.Name,
            list.Id
        );
    }
}
