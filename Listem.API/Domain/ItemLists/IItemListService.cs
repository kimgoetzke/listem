namespace Listem.API.Domain.ItemLists;

public interface IItemListService
{
    Task<List<ItemListResponse>> GetAllAsync(string userId);
    Task<ItemListResponse?> GetByIdAsync(string userId, string listId);
    Task<bool> ExistsAsync(string userId, string listId);
    Task<ItemListResponse?> CreateAsync(string userId, ItemListRequest itemList);
    Task<ItemListResponse?> UpdateAsync(string userId, string listId, ItemListRequest itemList);
    Task DeleteByIdAsync(string userId, string listId);
}
