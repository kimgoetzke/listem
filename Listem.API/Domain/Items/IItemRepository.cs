namespace Listem.API.Domain.Items;

internal interface IItemRepository
{
    Task<List<Item>> GetAllAsync();
    Task<List<Item>> GetAllByListIdAsync(string listId);
    Task<Item?> GetByIdAsync(string itemId);
    Task<Item?> CreateAsync(Item item);
    Task<Item?> UpdateAsync(Item item);
    Task<bool> DeleteAllByListIdAsync(string listId);
    Task<bool> DeleteByIdAsync(string listId, string itemId);
}
