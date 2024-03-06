namespace Listem.API.Domain.ItemLists;

public interface IItemListRepository
{
    Task<List<ItemList>> GetAllAsync(string userId);
    Task<ItemList?> GetByIdAsync(string userId, string listId);
    Task<bool> ExistsAsync(string userId, string listId);
    Task<ItemList?> CreateAsync(ItemList itemList);
    Task<ItemList?> UpdateAsync(ItemList itemList);
    Task<bool> DeleteByIdAsync(string userId, string listId);
}
