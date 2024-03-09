namespace Listem.API.Domain.Categories;

internal interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<List<Category>> GetAllByListIdAsync(string listId);
    Task<Category?> GetByIdAsync(string categoryId);
    Task<Category?> CreateAsync(Category category);
    Task<Category?> UpdateAsync(Category category);
    Task<bool> DeleteAllByListIdAsync(string listId);
    Task<bool> DeleteAllExceptDefaultByListIdAsync(string listId, string defaultCategoryId);
    Task<bool> DeleteByIdAsync(string listId, string categoryId);
}
