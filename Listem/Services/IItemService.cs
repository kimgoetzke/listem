using Listem.Models;

namespace Listem.Services;

public interface IItemService : IService
{
    ServiceType IService.Type => ServiceType.Item;
    Task<List<Item>> GetAsync();
    Task CreateOrUpdateAsync(Item item);
    Task DeleteAsync(Item item);
    Task DeleteAllAsync();
    Task UpdateAllToDefaultStoreAsync();
    Task UpdateAllUsingStoreAsync(string storeName);
}
