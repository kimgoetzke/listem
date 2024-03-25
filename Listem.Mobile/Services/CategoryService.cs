using Listem.Mobile.Models;
using Microsoft.Extensions.Logging;
using Realms;

namespace Listem.Mobile.Services;

public class CategoryService(ILogger<CategoryService> logger) : ICategoryService
{
    private readonly Realm _realm = RealmService.GetMainThreadRealm();

    public async Task CreateAsync(Category category, List list)
    {
        if (list.Categories.Contains(category))
        {
            logger.LogInformation(
                "Cannot add category '{Name}' - it already exists",
                category.Name
            );
            return;
        }
        await _realm.WriteAsync(() => list.Categories.Add(category));
        logger.LogInformation("Added: {Category}", category.ToLoggableString());
    }

    public async Task CreateAllAsync(IList<Category> categories, List list)
    {
        await _realm.WriteAsync(() =>
        {
            foreach (var category in categories)
            {
                if (list.Categories.Contains(category))
                {
                    logger.LogInformation(
                        "Cannot add category '{Name}' - it already exists",
                        category.Name
                    );
                    continue;
                }
                list.Categories.Add(category);
            }
        });
    }

    public async Task DeleteAsync(Category category)
    {
        logger.LogInformation("Removing: {Category}", category.ToLoggableString());
        await _realm.WriteAsync(() => _realm.Remove(category));
    }

    public async Task ResetAsync(List list)
    {
        logger.LogInformation("Resetting categories for list {Name}", list.Name);
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
