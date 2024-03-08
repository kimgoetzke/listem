using Listem.API.Contracts;
using Listem.API.Exceptions;
using Listem.API.Utilities;

namespace Listem.API.Domain.Categories;

internal class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    private const string DefaultCategoryName = ICategoryService.DefaultCategoryName;

    public async Task<List<CategoryResponse>> GetAllAsync(string userId)
    {
        var result = await categoryRepository.GetAllAsync(userId);
        return result.Select(c => c.ToResponse()).ToList();
    }

    public async Task<List<CategoryResponse>> GetAllByListIdAsync(string userId, string listId)
    {
        var result = await categoryRepository.GetAllByListIdAsync(userId, listId);
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
        string userId,
        string listId,
        string categoryId,
        CategoryRequest categoryRequest
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
        var result = categories.FirstOrDefault(c => c.Name == DefaultCategoryName);
        if (result is null)
        {
            throw new ServerErrorException(
                $"Default category '{DefaultCategoryName}' does not exist in list {listId} but it must always exist"
            );
        }

        Logger.Log($"Retrieved default category: {result}");
        return result.ToResponse();
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
