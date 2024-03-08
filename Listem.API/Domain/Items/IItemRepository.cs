namespace Listem.API.Domain.Items;

public interface IItemRepository
{
    Task<List<Item>> GetAllAsync(string userId);
    Task<List<Item>> GetAllByListIdAsync(string userId, string listId);
    Task<Item?> GetByIdAsync(string userId, string itemId);
    Task<Item?> CreateAsync(Item item);
    Task<Item?> UpdateAsync(Item item);
    Task<bool> DeleteAllByListIdAsync(string userId, string listId);
    Task<bool> DeleteByIdAsync(string userId, string listId, string itemId);
}
