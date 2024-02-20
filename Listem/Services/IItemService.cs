using Listem.Models;

namespace Listem.Services;

public interface IItemService : IService
{
    ServiceType IService.Type => ServiceType.Item;
    Task<List<ObservableItem>> GetAllAsync();
    Task<List<ObservableItem>> GetAllByListIdAsync(string listId);
    Task CreateOrUpdateAsync(ObservableItem observableItem);
    Task DeleteAsync(ObservableItem observableItem);
    Task DeleteAllByListIdAsync(string listId);
    Task UpdateAllToDefaultCategoryAsync(string listId);
    Task UpdateAllToCategoryAsync(string categoryName, string listId);
}
