using Listem.Mobile.Models;
using Microsoft.Extensions.Logging;
using Item = Listem.Mobile.Models.Item;

namespace Listem.Mobile.Services;

public class ItemService(IDatabaseProvider db, ILogger<ItemService> logger) : IItemService
{
  public async Task<List<ObservableItem>> GetAllByListIdAsync(string listId)
  {
    var connection = await db.GetConnection();
    var items = await connection.Table<Item>().Where(i => i.ListId == listId).ToListAsync();
    return ConvertToObservableItems(items);
  }

  private static List<ObservableItem> ConvertToObservableItems(List<Item> items)
  {
    return items.Select(ObservableItem.From).ToList();
  }

  public async Task CreateOrUpdateAsync(ObservableItem observableItem)
  {
    var connection = await db.GetConnection();
    var item = observableItem.ToItem();
    var allItems = await connection.Table<Item>().ToListAsync();
    var existingItem = allItems.FirstOrDefault(i => i.Id == observableItem.Id);
    if (existingItem != null)
    {
      await connection.UpdateAsync(item);
      logger.Info("Updated item: {Item}", item.ToLoggableString());
      return;
    }

    await connection.InsertAsync(item);
    logger.Info("Added item: {Item}", item.ToLoggableString());
  }

  public async Task DeleteAsync(ObservableItem observableItem)
  {
    logger.Info("Removing item: {Title} {ID}", observableItem.Title, observableItem.Id);
    var connection = await db.GetConnection();
    var item = observableItem.ToItem();
    await connection.DeleteAsync(item);
  }

  public async Task DeleteAllByListIdAsync(string listId)
  {
    var connection = await db.GetConnection();
    var items = await connection.Table<Item>().Where(i => i.ListId == listId).ToListAsync();
    foreach (var item in items)
    {
      await connection.DeleteAsync(item);
    }
    logger.Info("Removed all items from list {ListID}", listId);
  }

  public async Task UpdateAllToDefaultCategoryAsync(string listId)
  {
    var connection = await db.GetConnection();
    var items = await connection.Table<Item>().Where(i => i.ListId == listId).ToListAsync();
    foreach (var item in items.Where(item => item.CategoryName != Constants.DefaultCategoryName))
    {
      logger.Info("Updating item to use default category: {Item}", item.ToLoggableString());
      item.CategoryName = Constants.DefaultCategoryName;
      await connection.UpdateAsync(item);
    }

    logger.Info("Updated all items to use default category");
  }

  public async Task UpdateAllToCategoryAsync(string categoryName, string listId)
  {
    var count = 0;
    var connection = await db.GetConnection();
    var items = await connection.Table<Item>().Where(i => i.ListId == listId).ToListAsync();
    foreach (var item in items.Where(item => item.CategoryName == categoryName))
    {
      item.CategoryName = Constants.DefaultCategoryName;
      await connection.UpdateAsync(item);
      count++;
    }
    logger.Info(
      "Updated {Count} item(s) with category {Category} in {Id} to use default category",
      count,
      categoryName,
      listId
    );
  }
}
