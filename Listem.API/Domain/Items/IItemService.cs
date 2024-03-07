namespace Listem.API.Domain.Items;

public interface IItemService
{
    Task<List<ItemResponse>> GetAllAsync(string userId);
    Task<List<ItemResponse>> GetAllByListIdAsync(string userId, string listId);
    Task<ItemResponse?> CreateAsync(string userId, string listId,ItemRequest item);
    Task<ItemResponse?> UpdateAsync(string userId, string listId, string itemId, ItemRequest item);
    Task DeleteAllByListIdAsync(string userId, string listId);
    Task DeleteByIdAsync(string userId, string listId, string itemId);

    // TODO: Move methods below to CategoryService
    // Task UpdateAllToDefaultCategoryAsync(string ownerId, string listId);
    // Task UpdateAllToCategoryAsync(string ownerId, string categoryId, string listId);
}
