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
      logger.Info("Added: {Item}", item.ToLog());
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
        logger.Info("Not updated because it doesn't exist: {Item}", item.ToLog());
        return;
      }
      item.Name = name ?? item.Name;
      item.OwnedBy = ownedBy ?? item.OwnedBy;
      if (sharedWith != null)
      {
        item.SharedWith.Clear();
        foreach (var id in sharedWith)
        {
          item.SharedWith.Add(id);
        }
      }
      if (category != null)
      {
        item.Category = new Category { Name = category.Name };
      }
      item.Quantity = quantity ?? item.Quantity;
      item.IsImportant = isImportant ?? item.IsImportant;
      item.UpdatedOn = DateTimeOffset.Now.ToUniversalTime();
      logger.Info("Updated: {Item}", item.ToLog());
    });
  }

  public async Task DeleteAsync(Item item)
  {
    logger.Info("Removing: {Item}", item.ToLog());
    await _realm.WriteAsync(() => _realm.Remove(item));
  }

  public async Task DeleteAllInListAsync(List list)
  {
    await _realm.WriteAsync(() =>
    {
      _realm.RemoveRange(list.Items);
    });
    logger.Info("Removed all items in list '{Name}' {Id}", list.Name, list.Id);
  }

  public async Task ResetAllToDefaultCategoryAsync(List list)
  {
    var count = 0;
    await _realm.WriteAsync(() =>
    {
      var defaultCategory = list.Categories.First(c => c.Name == Constants.DefaultCategoryName);
      foreach (var item in list.Items)
      {
        item.Category = new Category { Name = defaultCategory.Name };
        count++;
      }
    });
    logger.Info(
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
      var defaultCategory = list.Categories.First(c => c.Name == Constants.DefaultCategoryName);
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
    logger.Info(
      "Updated {Count} item(s) with category '{Category}' in list '{Name}' {Id} to use default category",
      count,
      category.Name,
      list.Name,
      list.Id
    );
  }
}
