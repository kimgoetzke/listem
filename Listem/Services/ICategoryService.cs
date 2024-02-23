using Listem.Models;

namespace Listem.Services;

public interface ICategoryService : IService
{
    ServiceType IService.Type => ServiceType.Category;
    Task<ObservableCategory> GetDefaultCategory(string listId);
    Task<List<ObservableCategory>> GetAllAsync();
    Task<List<ObservableCategory>> GetAllByListIdAsync(string listId);
    Task CreateOrUpdateAsync(ObservableCategory observableCategory);
    Task DeleteAsync(ObservableCategory observableCategory);
    Task DeleteAllByListIdAsync(string listId);

    const string DefaultCategoryName = "(Default)";
}
