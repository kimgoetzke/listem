using Listem.Models;

namespace Listem.Services;

public interface ICategoryService : IService
{
    ServiceType IService.Type => ServiceType.Category;
    Task<Category> GetDefaultCategory();
    Task<List<Category>> GetAllAsync();
    Task CreateOrUpdateAsync(Category store);
    Task DeleteAsync(Category store);
    Task DeleteAllAsync();

    const string DefaultCategoryName = "Any";
}
