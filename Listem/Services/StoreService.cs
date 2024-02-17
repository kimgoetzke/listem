using Listem.Models;
using Listem.Utilities;
using SQLite;

namespace Listem.Services;

public class StoreService(IDatabaseProvider db) : IStoreService
{
    private ConfigurableStore? _defaultStore;

    public async Task<ConfigurableStore> GetDefaultStore()
    {
        if (_defaultStore == null)
        {
            var connection = await db.GetConnection();
            var loadedStore = await connection
                .Table<ConfigurableStore>()
                .FirstAsync(s => s.Name == IStoreService.DefaultStoreName);
            _defaultStore = loadedStore;
        }

        if (_defaultStore == null)
            throw new NullReferenceException("There is no default store in the database");

        return _defaultStore;
    }

    public async Task<List<ConfigurableStore>> GetAllAsync()
    {
        var connection = await db.GetConnection();
        return await connection.Table<ConfigurableStore>().ToListAsync();
    }

    public async Task CreateOrUpdateAsync(ConfigurableStore store)
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

    public async Task DeleteAsync(ConfigurableStore store)
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
        var stores = await connection.Table<ConfigurableStore>().ToListAsync();
        foreach (var store in stores.Where(store => store.Name != IStoreService.DefaultStoreName))
        {
            await connection.DeleteAsync(store);
        }
    }
}
