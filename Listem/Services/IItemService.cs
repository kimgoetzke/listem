using Listem.Models;

namespace Listem.Services;

public interface IItemService : IService
{
    ServiceType IService.Type => ServiceType.Item;
    Task<List<Item>> GetAsync();
    Task CreateOrUpdateAsync(Item item);
    Task DeleteAsync(Item item);
    Task DeleteAllAsync();
    Task UpdateAllToDefaultCategoryAsync();
    Task UpdateAllUsingCategoryAsync(string storeName);
}
