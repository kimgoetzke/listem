using Listem.API.Contracts;

namespace Listem.API.Domain.Categories;

public interface ICategoryService
{
    const string DefaultCategoryName = "None";
    Task<List<CategoryResponse>> GetAllAsync();
    Task<List<CategoryResponse>> GetAllByListIdAsync(string listId);
    Task<CategoryResponse> GetDefaultCategory(string listId);
    Task<CategoryResponse?> CreateAsync(string userId, string listId, CategoryRequest category);
    Task<CategoryResponse?> UpdateAsync(string listId, string categoryId, CategoryRequest category);
    Task DeleteAllByListIdAsync(string listId, string? defaultCategoryId = null);
    Task DeleteByIdAsync(string listId, string categoryId);
}
