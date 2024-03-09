using Listem.API.Domain.Items;
using Listem.API.Middleware;
using Microsoft.EntityFrameworkCore;

namespace Listem.API.Domain.Categories;

#pragma warning disable CS1998
internal class CategoryRepository(
    CategoryDbContext dbContext,
    ILogger<ItemRepository> logger,
    IRequestContext reqContext
) : ICategoryRepository
{
    public Task<List<Category>> GetAllAsync()
    {
        var categories = dbContext.Categories.Where(i => i.OwnerId == reqContext.UserId).ToList();
        logger.LogInformation("Retrieved {Count} categories", categories.Count);
        return Task.FromResult(categories);
    }

    public Task<List<Category>> GetAllByListIdAsync(string listId)
    {
        var categories = dbContext
            .Categories.Where(i => i.ListId == listId && i.OwnerId == reqContext.UserId)
            .ToList();
        logger.LogInformation(
            "Retrieved {Count} categories for list {ListId}",
            categories.Count,
            listId
        );
        return Task.FromResult(categories);
    }

    public Task<Category?> GetByIdAsync(string categoryId)
    {
        var category = dbContext.Categories.FirstOrDefault(i =>
            i.Id == categoryId && i.OwnerId == reqContext.UserId
        );
        logger.LogInformation("Retrieved category: {Category}", category?.ToString() ?? "null");
        return Task.FromResult(category);
    }

    public async Task<Category?> CreateAsync(Category category)
    {
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Added category: {Category}", category);
        return dbContext.Categories.FirstOrDefault(i => i.Id == category.Id);
    }

    public async Task<Category?> UpdateAsync(Category category)
    {
        var existingCategory = dbContext.Categories.FirstOrDefault(i => i.Id == category.Id);

        if (existingCategory is null)
            return null;

        existingCategory.Name = category.Name;
        existingCategory.UpdatedOn = DateTime.Now;
        logger.LogInformation("Updated category: {Category}", existingCategory);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("The category was updated by another process");
        }
        return existingCategory;
    }

    public async Task<bool> DeleteAllByListIdAsync(string listId)
    {
        var toDelete = dbContext.Categories.Where(i =>
            i.ListId == listId && i.OwnerId == reqContext.UserId
        );
        logger.LogInformation(
            "Removing all {Count} categories in list {ListId} by {UserId}",
            toDelete.Count(),
            listId,
            reqContext.UserId
        );
        dbContext.Categories.RemoveRange(toDelete);
        return await dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAllExceptDefaultByListIdAsync(
        string listId,
        string defaultCategoryId
    )
    {
        var toDelete = dbContext.Categories.Where(i =>
            i.ListId == listId && i.OwnerId == reqContext.UserId && i.Id != defaultCategoryId
        );
        logger.LogInformation(
            "Removing {Count} categories (all except default category) in list {ListId} by {UserId}",
            toDelete.Count(),
            listId,
            reqContext.UserId
        );
        dbContext.Categories.RemoveRange(toDelete);
        return await dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteByIdAsync(string listId, string categoryId)
    {
        logger.LogInformation(
            "Removing category: {CategoryId} by {UserId}",
            categoryId,
            reqContext.UserId
        );
        var toDelete = dbContext.Categories.Where(i =>
            i.Id == categoryId && i.ListId == listId && i.OwnerId == reqContext.UserId
        );
        dbContext.Categories.RemoveRange(toDelete);
        return await dbContext.SaveChangesAsync() > 0;
    }
#pragma warning restore CS1998
}
