using Listem.Mobile.Models;
using Microsoft.Extensions.Logging;

namespace Listem.Mobile.Services;

public class CategoryService(ILogger<CategoryService> logger) : ICategoryService
{
  public async Task CreateAsync(Category category, List list)
  {
    if (list.Categories.Contains(category))
    {
      logger.Info("Cannot add category '{Name}' - it already exists", category.Name);
      return;
    }
    var realm = RealmService.GetMainThreadRealm();
    await realm.WriteAsync(() => list.Categories.Add(category));
    logger.Info("Added: {Category}", category.ToLog());
  }

  public async Task CreateAllAsync(IList<Category> categories, List list)
  {
    var realm = RealmService.GetMainThreadRealm();
    await realm.WriteAsync(() =>
    {
      foreach (var category in categories)
      {
        if (list.Categories.Contains(category))
        {
          logger.Info("Cannot add category '{Name}' - it already exists", category.Name);
          continue;
        }
        list.Categories.Add(category);
      }
    });
  }

  public async Task DeleteAsync(Category category)
  {
    logger.Info("Removing: {Category}", category.ToLog());
    var realm = RealmService.GetMainThreadRealm();
    await realm.WriteAsync(() => realm.Remove(category));
  }

  public async Task ResetAsync(List list)
  {
    logger.Info("Resetting categories for list {Name}", list.Name);
    var realm = RealmService.GetMainThreadRealm();
    await realm.WriteAsync(() =>
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
