using Listem.Shared.Contracts;

namespace Listem.API.Domain.Lists;

public interface IListService
{
    Task<List<ListResponse>> GetAllAsync();
    Task<ListResponse?> GetByIdAsync(string listId);
    Task<bool> ExistsAsync(string listId);
    Task<ListResponse?> CreateAsync(string userId, ListRequest list);
    Task<ListResponse?> UpdateAsync(string listId, ListRequest list);
    Task DeleteByIdAsync(string listId);
}
