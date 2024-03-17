using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Item = Listem.Mobile.Models.Item;

namespace Listem.Mobile.Services;

public class OfflineItemService(IDatabaseProvider db) : IOfflineItemService
{
    public async Task<List<ObservableItem>> GetAllAsync()
    {
        var connection = await db.GetConnection();
        var items = await connection.Table<Item>().ToListAsync();
        return ConvertToObservableItems(items);
    }

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
            Logger.Log($"Updated item: {item.ToLoggableString()}");
            return;
        }

        await connection.InsertAsync(item);
        Logger.Log($"Added item: {item.ToLoggableString()}");
    }

    public async Task DeleteAsync(ObservableItem observableItem)
    {
        Logger.Log($"Removing item: {observableItem.Title} {observableItem.Id}");
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
        Logger.Log($"Removed all items from list {listId}");
    }

    public async Task UpdateAllToDefaultCategoryAsync(string listId)
    {
        var connection = await db.GetConnection();
        var items = await connection.Table<Item>().Where(i => i.ListId == listId).ToListAsync();
        foreach (
            var item in items.Where(item =>
                item.CategoryName != ICategoryService.DefaultCategoryName
            )
        )
        {
            Logger.Log($"Updating item to use default category: {item.ToLoggableString()}");
            item.CategoryName = ICategoryService.DefaultCategoryName;
            await connection.UpdateAsync(item);
        }

        Logger.Log($"Updated all items to use default category");
    }

    public async Task UpdateAllToCategoryAsync(string categoryName, string listId)
    {
        var connection = await db.GetConnection();
        var items = await connection.Table<Item>().Where(i => i.ListId == listId).ToListAsync();
        foreach (var item in items.Where(item => item.CategoryName == categoryName))
        {
            item.CategoryName = ICategoryService.DefaultCategoryName;
            await connection.UpdateAsync(item);
        }
    }
}
