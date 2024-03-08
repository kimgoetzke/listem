using Listem.API.Contracts;

namespace Listem.API.Domain.Items;

public interface IItemService
{
    Task<List<ItemResponse>> GetAllAsync(string userId);
    Task<List<ItemResponse>> GetAllByListIdAsync(string userId, string listId);
    Task<ItemResponse?> CreateAsync(string userId, string listId, ItemRequest item);
    Task<ItemResponse?> UpdateAsync(string userId, string listId, string itemId, ItemRequest item);
    Task UpdateToDefaultCategoryAsync(
        string userId,
        string listId,
        string defaultCategoryId,
        string currentCategoryId
    );
    Task UpdateToDefaultCategoryAsync(string userId, string listId, string defaultCategoryId);
    Task DeleteAllByListIdAsync(string userId, string listId);
    Task DeleteByIdAsync(string userId, string listId, string itemId);
}
