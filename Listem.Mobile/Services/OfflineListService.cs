using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using SQLite;
using static Listem.Shared.Constants;
using Models_Category = Listem.Mobile.Models.Category;

namespace Listem.Mobile.Services;

public class OfflineListService(IDatabaseProvider db) : IOfflineListService
{
    public async Task<List<ObservableList>> GetAllAsync()
    {
        var connection = await db.GetConnection();
        var lists = await connection.Table<List>().ToListAsync();
        return ConvertToObservableItemLists(lists);
    }

    private static List<ObservableList> ConvertToObservableItemLists(List<List> lists)
    {
        return lists.Select(ObservableList.From).ToList();
    }

    public async Task CreateOrUpdateAsync(ObservableList observableList)
    {
        var connection = await db.GetConnection();
        var list = observableList.ToItemList();
        var existingList = await connection
            .Table<List>()
            .Where(i => i.Id == observableList.Id)
            .FirstOrDefaultAsync();
        if (existingList != null)
        {
            await connection.UpdateAsync(list);
            Logger.Log($"Updated list: {list.ToLoggableString()}");
            return;
        }

        await connection.InsertAsync(list);
        Logger.Log($"Added list: {list.ToLoggableString()}");
        await CreateDefaultCategory(connection, list.Id);
    }

    private static async Task CreateDefaultCategory(SQLiteAsyncConnection connection, string listId)
    {
        var existingDefaultCategory = await connection
            .Table<Models_Category>()
            .Where(i => i.ListId == listId && i.Name == DefaultCategoryName)
            .FirstOrDefaultAsync();

        if (existingDefaultCategory != null)
        {
            Logger.Log($"Default category already exists for list {listId} - skipping creation");
            return;
        }

        var observableCategory = new ObservableCategory(listId) { Name = DefaultCategoryName };
        var category = observableCategory.ToCategory();
        await connection.InsertAsync(category).ConfigureAwait(false);
        Logger.Log($"Added category '{DefaultCategoryName}' to list {listId}");
    }

    public async Task DeleteAsync(ObservableList observableList)
    {
        // TODO: Delete all categories and items associated with this list
        Logger.Log($"Removing list: '{observableList.Name}' {observableList.Id}");
        var connection = await db.GetConnection();
        var list = observableList.ToItemList();
        await connection.DeleteAsync(list);
    }

    public async Task DeleteAllAsync()
    {
        // TODO: Delete all categories and items associated with all lists
        var connection = await db.GetConnection();
        var allLists = await connection.Table<List>().ToListAsync();
        foreach (var list in allLists)
        {
            await connection.DeleteAsync(list);
        }

        Logger.Log($"Removed all lists");
    }
}
