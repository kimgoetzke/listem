using Listem.API.Contracts;

namespace Listem.API.Domain.Items;

public interface IItemService
{
    Task<List<ItemResponse>> GetAllAsync();
    Task<List<ItemResponse>> GetAllByListIdAsync(string listId);
    Task<ItemResponse?> CreateAsync(string userId, string listId, ItemRequest item);
    Task<ItemResponse?> UpdateAsync(string listId, string itemId, ItemRequest item);
    Task UpdateToDefaultCategoryAsync(
        string listId,
        string defaultCategoryId,
        string currentCategoryId
    );
    Task UpdateToDefaultCategoryAsync(string listId, string defaultCategoryId);
    Task DeleteAllByListIdAsync(string listId);
    Task DeleteByIdAsync(string listId, string itemId);
}
