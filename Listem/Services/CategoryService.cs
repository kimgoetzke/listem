using Listem.Models;
using Listem.Utilities;
using SQLite;

namespace Listem.Services;

public class CategoryService(IDatabaseProvider db) : ICategoryService
{
    private Category? _defaultStore;

    public async Task<Category> GetDefaultCategory()
    {
        if (_defaultStore == null)
        {
            var connection = await db.GetConnection();
            var loadedStore = await connection
                .Table<Category>()
                .FirstAsync(s => s.Name == ICategoryService.DefaultCategoryName);
            _defaultStore = loadedStore;
        }

        if (_defaultStore == null)
            throw new NullReferenceException("There is no default store in the database");

        return _defaultStore;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        var connection = await db.GetConnection();
        return await connection.Table<Category>().ToListAsync();
    }

    public async Task CreateOrUpdateAsync(Category store)
    {
        var connection = await db.GetConnection();
        if (store.Id != 0)
        {
            await connection.UpdateAsync(store);
            return;
        }

        await connection.InsertAsync(store);
        Logger.Log($"Added or updated store: {store.ToLoggableString()}");
    }

    public async Task DeleteAsync(Category store)
    {
        Logger.Log($"Removing store: {store.ToLoggableString()}");
        var connection = await db.GetConnection();
        await connection.DeleteAsync(store);
    }

    public async Task DeleteAllAsync()
    {
        var connection = await db.GetConnection();
        await RemoveAllExceptDefaultStore(connection);
        Logger.Log($"Reset all stores");
    }

    private static async Task RemoveAllExceptDefaultStore(SQLiteAsyncConnection connection)
    {
        var stores = await connection.Table<Category>().ToListAsync();
        foreach (var store in stores.Where(store => store.Name != ICategoryService.DefaultCategoryName))
        {
            await connection.DeleteAsync(store);
        }
    }
}
