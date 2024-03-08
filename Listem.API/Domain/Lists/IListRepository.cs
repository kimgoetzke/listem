namespace Listem.API.Domain.Lists;

public interface IListRepository
{
    Task<List<List>> GetAllAsync(string userId);
    Task<List?> GetByIdAsync(string userId, string listId);
    Task<bool> ExistsAsync(string userId, string listId);
    Task<List?> CreateAsync(List list);
    Task<List?> UpdateAsync(List list);
    Task<bool> DeleteByIdAsync(string userId, string listId);
}
