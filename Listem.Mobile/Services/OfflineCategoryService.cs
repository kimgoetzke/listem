using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using SQLite;
using static Listem.Shared.Constants;
using Category = Listem.Mobile.Models.Category;

namespace Listem.Mobile.Services;

public class OfflineCategoryService(IDatabaseProvider db) : IOfflineCategoryService
{
    private ObservableCategory? _defaultCategory;

    public async Task<ObservableCategory> GetDefaultCategory(string listId)
    {
        if (_defaultCategory == null)
        {
            var connection = await db.GetConnection();
            var loaded = await connection
                .Table<Category>()
                .FirstAsync(l => l.Name == DefaultCategoryName && l.ListId == listId);
            _defaultCategory = ObservableCategory.From(loaded);
        }

        if (_defaultCategory == null)
            throw new NullReferenceException("This list does not have default category");

        return _defaultCategory;
    }

    public async Task<List<ObservableCategory>> GetAllAsync()
    {
        var connection = await db.GetConnection();
        var categories = await connection.Table<Category>().ToListAsync();
        return ConvertToObservableItems(categories);
    }

    public async Task<List<ObservableCategory>> GetAllByListIdAsync(string listId)
    {
        var connection = await db.GetConnection();
        var categories = await connection
            .Table<Category>()
            .Where(c => c.ListId == listId)
            .ToListAsync();
        return ConvertToObservableItems(categories);
    }

    private static List<ObservableCategory> ConvertToObservableItems(List<Category> categories)
    {
        return categories.Select(ObservableCategory.From).ToList();
    }

    public async Task CreateOrUpdateAsync(ObservableCategory observableCategory)
    {
        var connection = await db.GetConnection();
        var category = observableCategory.ToCategory();
        var existingCategory = await connection
            .Table<Category>()
            .Where(c => c.Id == observableCategory.Id)
            .FirstOrDefaultAsync();
        if (existingCategory != null)
        {
            await connection.UpdateAsync(category);
            Logger.Log($"Updated category: {category.ToLoggableString()}");
            return;
        }

        await connection.InsertAsync(category);
        Logger.Log($"Added category: {category.ToLoggableString()}");
    }

    public async Task DeleteAsync(ObservableCategory observableCategory)
    {
        Logger.Log($"Removing category: {observableCategory.ToLoggableString()}");
        var connection = await db.GetConnection();
        var category = observableCategory.ToCategory();
        await connection.DeleteAsync(category);
    }

    public async Task DeleteAllByListIdAsync(string listId)
    {
        var connection = await db.GetConnection();
        await RemoveAllExceptDefaultCategory(connection, listId);
        Logger.Log($"Reset all categories for list {listId}");
    }

    private static async Task RemoveAllExceptDefaultCategory(
        SQLiteAsyncConnection connection,
        string listId
    )
    {
        var categories = await connection
            .Table<Category>()
            .Where(c => c.ListId == listId)
            .ToListAsync();
        foreach (var category in categories.Where(c => c.Name != DefaultCategoryName))
        {
            await connection.DeleteAsync(category);
        }
    }
}
