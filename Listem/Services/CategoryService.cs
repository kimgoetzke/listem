using Listem.Models;
using Listem.Utilities;
using SQLite;
using static Listem.Services.ICategoryService;

namespace Listem.Services;

public class CategoryService(IDatabaseProvider db) : ICategoryService
{
    private ObservableCategory? _defaultStore;

    public async Task<ObservableCategory> GetDefaultCategory(string listId)
    {
        if (_defaultStore == null)
        {
            var connection = await db.GetConnection();
            var loaded = await connection
                .Table<Category>()
                .FirstAsync(l => l.Name == DefaultCategoryName && l.ListId == listId);
            _defaultStore = ObservableCategory.From(loaded);
        }

        if (_defaultStore == null)
            throw new NullReferenceException("There is no default category in the database");

        return _defaultStore;
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
        var category = Category.From(observableCategory);
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
        var category = Category.From(observableCategory);
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
