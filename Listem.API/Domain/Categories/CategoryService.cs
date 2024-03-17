using Listem.API.Exceptions;
using Listem.Shared.Contracts;

namespace Listem.API.Domain.Categories;

internal class CategoryService(
    ICategoryRepository categoryRepository,
    ILogger<CategoryService> logger
) : ICategoryService
{
    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        var result = await categoryRepository.GetAllAsync();
        return result.Select(c => c.ToResponse()).ToList();
    }

    public async Task<List<CategoryResponse>> GetAllByListIdAsync(string listId)
    {
        var result = await categoryRepository.GetAllByListIdAsync(listId);
        if (result.Count == 0)
        {
            throw new NotFoundException($"List {listId} does not exist");
        }
        return result.Select(c => c.ToResponse()).ToList();
    }

    public async Task<CategoryResponse?> CreateAsync(
        string userId,
        string listId,
        CategoryRequest category
    )
    {
        var toCreate = Category.From(category, userId, listId);
        var result = await categoryRepository.CreateAsync(toCreate);
        return result is not null
            ? result.ToResponse()
            : throw new ConflictException("Category cannot be created, it already exists");
    }

    public async Task<CategoryResponse?> UpdateAsync(
        string listId,
        string categoryId,
        CategoryRequest categoryRequest
    )
    {
        var existing = await categoryRepository.GetByIdAsync(categoryId);

        if (existing is null)
            throw new NotFoundException(
                $"Failed to update category {categoryId} because it does not exist"
            );

        if (existing.ListId != listId)
            throw new BadRequestException(
                $"Failed to update category {categoryId} because it does not belong to list {listId}"
            );

        existing.Update(categoryRequest);
        var result = await categoryRepository.UpdateAsync(existing);

        if (result is not null)
        {
            return result.ToResponse();
        }

        throw new NotFoundException(
            $"Failed to update category {categoryId} even though it was found"
        );
    }

    public async Task DeleteAllByListIdAsync(string listId, string? defaultCategoryId = null)
    {
        if (defaultCategoryId is not null)
        {
            if (
                !await categoryRepository.DeleteAllExceptDefaultByListIdAsync(
                    listId,
                    defaultCategoryId
                )
            )
            {
                logger.LogInformation(
                    "List {listId} only has a default category which cannot be deleted",
                    listId
                );
            }
        }
        else
        {
            if (!await categoryRepository.DeleteAllByListIdAsync(listId))
            {
                logger.LogWarning("There are no categories to delete in list {listId}", listId);
            }
        }
    }

    public async Task<CategoryResponse> GetDefaultCategory(string listId)
    {
        var categories = await categoryRepository.GetAllByListIdAsync(listId);
        var result = categories.FirstOrDefault(c => c.Name == Shared.Constants.DefaultCategoryName);
        if (result is null)
        {
            throw new ServerErrorException(
                $"Default category '{Shared.Constants.DefaultCategoryName}' does not exist in list {listId} but it must always exist"
            );
        }

        logger.LogInformation("Retrieved default category: {Category}", result);
        return result.ToResponse();
    }

    public async Task DeleteByIdAsync(string listId, string categoryId)
    {
        if (!await categoryRepository.DeleteByIdAsync(listId, categoryId))
        {
            throw new NotFoundException(
                $"Failed to delete category {categoryId} because it does not exist"
            );
        }
    }
}
