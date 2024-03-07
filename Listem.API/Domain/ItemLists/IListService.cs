namespace Listem.API.Domain.ItemLists;

public interface IListService
{
    Task<List<ListResponse>> GetAllAsync(string userId);
    Task<ListResponse?> GetByIdAsync(string userId, string listId);
    Task<bool> ExistsAsync(string userId, string listId);
    Task<ListResponse?> CreateAsync(string userId, ListRequest list);
    Task<ListResponse?> UpdateAsync(string userId, string listId, ListRequest list);
    Task DeleteByIdAsync(string userId, string listId);
}
