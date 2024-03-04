using Listem.API.Contracts;

namespace Listem.API.Repositories;

public interface IItemListRepository
{
    Task<List<ItemList>> GetAllAsync(string userId);
    Task<ItemList?> GetByIdAsync(string userId, string listId);
    Task<ItemList?> CreateAsync(ItemList itemList);
    Task<ItemList?> UpdateAsync(ItemList itemList);
    Task<bool> DeleteAsync(ItemList itemList);
    Task<bool> DeleteByIdAsync(string userId, string listId);
}
