namespace Listem.API.Domain.Categories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(string userId);
    Task<List<Category>> GetAllByListIdAsync(string userId, string listId);
    Task<Category?> GetByIdAsync(string userId, string categoryId);
    Task<Category?> CreateAsync(Category category);
    Task<Category?> UpdateAsync(Category category);
    Task<bool> DeleteAllByListIdAsync(string userId, string listId);
    Task<bool> DeleteAllExceptDefaultByListIdAsync(
        string userId,
        string listId,
        string defaultCategoryId
    );
    Task<bool> DeleteByIdAsync(string userId, string listId, string categoryId);
}
