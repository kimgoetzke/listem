using Listem.API.Contracts;
using Listem.API.Exceptions;
using Listem.API.Utilities;

namespace Listem.API.Domain.Categories;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    private const string DefaultCategoryName = ICategoryService.DefaultCategoryName;

    public async Task<List<CategoryResponse>> GetAllAsync(string userId)
    {
        var categories = await categoryRepository.GetAllAsync(userId);
        return categories.Select(CategoryResponse.FromCategory).ToList();
    }

    public async Task<List<CategoryResponse>> GetAllByListIdAsync(string userId, string listId)
    {
        var categories = await categoryRepository.GetAllByListIdAsync(userId, listId);
        if (categories.Count == 0)
        {
            throw new NotFoundException($"List {listId} does not exist");
        }
        return categories.Select(CategoryResponse.FromCategory).ToList();
    }

    public async Task<CategoryResponse?> CreateAsync(
        string userId,
        string listId,
        CategoryRequest category
    )
    {
        var toCreate = category.ToCategory(userId, listId);
        var result = await categoryRepository.CreateAsync(toCreate);
        return result is not null
            ? CategoryResponse.FromCategory(result)
            : throw new ConflictException("Category cannot be created, it already exists");
    }

    public async Task<CategoryResponse?> UpdateAsync(
        string userId,
        string listId,
        string categoryId,
        CategoryRequest requested
    )
    {
        var existing = await categoryRepository.GetByIdAsync(userId, categoryId);

        if (existing is null)
            throw new NotFoundException(
                $"Failed to update category {categoryId} because it does not exist"
            );

        if (existing.ListId != listId)
            throw new BadRequestException(
                $"Failed to update category {categoryId} because it does not belong to list {listId}"
            );

        var toUpdate = requested.ToCategory(existing);
        var result = await categoryRepository.UpdateAsync(toUpdate);

        if (result is not null)
        {
            return CategoryResponse.FromCategory(result);
        }

        throw new NotFoundException(
            $"Failed to update category {categoryId} even though it was found"
        );
    }

    public async Task DeleteAllByListIdAsync(
        string userId,
        string listId,
        string? defaultCategoryId = null
    )
    {
        if (defaultCategoryId is not null)
        {
            await categoryRepository.DeleteAllExceptDefaultByListIdAsync(
                userId,
                listId,
                defaultCategoryId
            );
        }
        else
        {
            if (!await categoryRepository.DeleteAllByListIdAsync(userId, listId))
            {
                throw new NotFoundException($"Failed to reset categories in list {listId}");
            }
        }
    }

    public async Task<CategoryResponse> GetDefaultCategory(string userId, string listId)
    {
        var categories = await categoryRepository.GetAllByListIdAsync(userId, listId);
        var defaultCategory = categories.FirstOrDefault(c => c.Name == DefaultCategoryName);
        if (defaultCategory is null)
        {
            throw new ServerErrorException(
                $"Default category '{DefaultCategoryName}' does not exist in list {listId} but it must always exist"
            );
        }

        Logger.Log($"Retrieved default category: {defaultCategory}");
        return CategoryResponse.FromCategory(defaultCategory);
    }

    public async Task DeleteByIdAsync(string userId, string listId, string categoryId)
    {
        if (!await categoryRepository.DeleteByIdAsync(userId, listId, categoryId))
        {
            throw new NotFoundException(
                $"Failed to delete category {categoryId} because it does not exist"
            );
        }
    }
}
