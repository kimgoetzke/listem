using Listem.API.Utilities;

namespace Listem.API.Domain.Categories;

#pragma warning disable CS1998
public class PlaceholderCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories = [];

    public Task<List<Category>> GetAllAsync(string userId)
    {
        return Task.FromResult(_categories.FindAll(i => i.OwnerId == userId));
    }

    public Task<List<Category>> GetAllByListIdAsync(string userId, string listId)
    {
        var categories = _categories.FindAll(i => i.ListId == listId && i.OwnerId == userId);
        Logger.Log($"Retrieved {categories.Count} categories for list {listId}");
        return Task.FromResult(categories);
    }

    public Task<Category?> GetByIdAsync(string userId, string categoryId)
    {
        var category = _categories.FirstOrDefault(i => i.Id == categoryId && i.OwnerId == userId);
        Logger.Log($"Retrieved category: {category?.ToString() ?? "null"}");
        return Task.FromResult(category);
    }

    public async Task<Category?> CreateAsync(Category category)
    {
        _categories.Add(category);
        Logger.Log($"Added category: {category}");
        return _categories.FirstOrDefault(i => i.Id == category.Id);
    }

    public async Task<Category?> UpdateAsync(Category category)
    {
        var existingCategory = _categories.FirstOrDefault(i => i.Id == category.Id);

        if (existingCategory is null)
            return null;

        existingCategory.Name = category.Name;
        existingCategory.UpdatedOn = DateTime.Now;
        Logger.Log($"Updated category: {existingCategory}");
        return existingCategory;
    }

    public async Task<bool> DeleteAllByListIdAsync(string userId, string listId)
    {
        Logger.Log($"Removing all categories in list {listId} by {userId}");
        return _categories.RemoveAll(i => i.ListId == listId && i.OwnerId == userId) > 0;
    }

    public async Task<bool> DeleteAllExceptDefaultByListIdAsync(
        string userId,
        string listId,
        string defaultCategoryId
    )
    {
        Logger.Log(
            $"Removing all categories, except default category, in list {listId} by {userId}"
        );
        return _categories.RemoveAll(i =>
                i.ListId == listId && i.OwnerId == userId && i.Id != defaultCategoryId
            ) > 0;
    }

    public async Task<bool> DeleteByIdAsync(string userId, string listId, string categoryId)
    {
        Logger.Log($"Removing category: {categoryId} by {userId}");
        return _categories.RemoveAll(i =>
                i.Id == categoryId && i.ListId == listId && i.OwnerId == userId
            ) > 0;
    }
#pragma warning restore CS1998
}
