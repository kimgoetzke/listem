using Listem.Contracts;

namespace Listem.API.Repositories;

public interface IItemListRepository
{
    Task<List<ItemList>> GetAllAsync();
    Task<ItemList?> GetByIdAsync(string id);
    Task<ItemList?> CreateAsync(ItemList itemList);
    Task<ItemList?> UpdateAsync(ItemList itemList);
    Task<bool> DeleteAsync(ItemList itemList);
}
