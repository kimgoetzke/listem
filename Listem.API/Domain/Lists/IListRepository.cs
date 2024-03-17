namespace Listem.API.Domain.Lists;

internal interface IListRepository
{
    Task<List<List>> GetAllAsync();
    Task<List?> GetByIdAsync(string listId);
    Task<bool> ExistsAsync(string listId);
    Task<List?> CreateAsync(List list);
    Task<List?> UpdateAsync(List list);
    Task<bool> DeleteByIdAsync(string listId);
}
