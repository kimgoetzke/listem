using Listem.API.Contracts;

namespace Listem.API.Domain.Categories;

public interface ICategoryService
{
    const string DefaultCategoryName = "None";
    Task<List<CategoryResponse>> GetAllAsync(string userId);
    Task<List<CategoryResponse>> GetAllByListIdAsync(string userId, string listId);
    Task<CategoryResponse> GetDefaultCategory(string userId, string listId);
    Task<CategoryResponse?> CreateAsync(string userId, string listId, CategoryRequest category);
    Task<CategoryResponse?> UpdateAsync(
        string userId,
        string listId,
        string categoryId,
        CategoryRequest category
    );
    Task DeleteAllByListIdAsync(string userId, string listId, string? defaultCategoryId = null);
    Task DeleteByIdAsync(string userId, string listId, string categoryId);
}
