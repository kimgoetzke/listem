using Listem.Models;
using Listem.Utilities;

namespace Listem.Services;

public class ItemListService(IDatabaseProvider db) : IItemListService
{
    public async Task<List<ObservableItemList>> GetAllAsync()
    {
        var connection = await db.GetConnection();
        var items = await connection.Table<ItemList>().ToListAsync();
        return ConvertToObservableItemLists(items);
    }

    private static List<ObservableItemList> ConvertToObservableItemLists(List<ItemList> items)
    {
        return items.Select(ObservableItemList.From).ToList();
    }

    public async Task CreateOrUpdateAsync(ObservableItemList observableItemList)
    {
        var connection = await db.GetConnection();
        var list = ItemList.From(observableItemList);
        var existingList = await connection
            .Table<ItemList>()
            .Where(i => i.Id == observableItemList.Id)
            .FirstOrDefaultAsync();
        if (existingList != null)
        {
            await connection.UpdateAsync(list);
            Logger.Log($"Updated list: {list.ToLoggableString()}");
            await CreateDefaultCategory(list.Id);
            return;
        }

        await connection.InsertAsync(list);
        Logger.Log($"Added list: {list.ToLoggableString()}");
        await CreateDefaultCategory(list.Id);
    }

    public async Task DeleteAsync(ObservableItemList observableItemList)
    {
        // TODO: Delete all categories and items associated with this list
        Logger.Log($"Removing list: '{observableItemList.Name}' {observableItemList.Id}");
        var connection = await db.GetConnection();
        var list = ItemList.From(observableItemList);
        await connection.DeleteAsync(list);
    }

    public async Task DeleteAllAsync()
    {
        // TODO: Delete all categories and items associated with all lists
        var connection = await db.GetConnection();
        var allLists = await connection.Table<ItemList>().ToListAsync();
        foreach (var list in allLists)
        {
            await connection.DeleteAsync(list);
        }

        Logger.Log($"Removed all lists");
    }

    private async Task CreateDefaultCategory(string listId)
    {
        var connection = await db.GetConnection();
        if (await connection.Table<Category>().CountAsync() != 0)
            return;

        var observableCategory = new ObservableCategory(listId)
        {
            Name = ICategoryService.DefaultCategoryName
        };
        var category = Category.From(observableCategory);
        await connection.InsertAsync(category).ConfigureAwait(false);
        Logger.Log($"Added category '{ICategoryService.DefaultCategoryName}' to list {listId}");
    }
}
