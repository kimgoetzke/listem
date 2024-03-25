using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Realms;

namespace Listem.Mobile.Services;

public class CategoryService : ICategoryService
{
    private readonly Realm _realm = RealmService.GetMainThreadRealm();

    public async Task CreateAsync(Category category, List list)
    {
        if (list.Categories.Contains(category))
        {
            Logger.Log($"Cannot add category '{category.Name}' - it already exists");
            return;
        }
        await _realm.WriteAsync(() => list.Categories.Add(category));
        Logger.Log($"Added: {category.ToLoggableString()}");
    }

    public async Task CreateAllAsync(IList<Category> categories, List list)
    {
        await _realm.WriteAsync(() =>
        {
            foreach (var category in categories)
            {
                if (list.Categories.Contains(category))
                {
                    Logger.Log($"Cannot add category '{category.Name}' - it already exists");
                    continue;
                }
                list.Categories.Add(category);
            }
        });
    }

    public async Task DeleteAsync(Category category)
    {
        Logger.Log($"Removing: {category.ToLoggableString()}");
        await _realm.WriteAsync(() => _realm.Remove(category));
    }

    public async Task ResetAsync(List list)
    {
        Logger.Log($"Resetting categories for list {list.Name}");
        await _realm.WriteAsync(() =>
        {
            var toRemove = list
                .Categories.AsQueryable()
                .Where(category => category.Name != Constants.DefaultCategoryName);
            foreach (var category in toRemove)
            {
                list.Categories.Remove(category);
            }
        });
    }
}
