using Listem.API.Contracts;
using Listem.API.Exceptions;

namespace Listem.API.Domain.Items;

public class ItemService(IItemRepository itemRepository) : IItemService
{
    public async Task<List<ItemResponse>> GetAllAsync(string userId)
    {
        var categories = await itemRepository.GetAllAsync(userId);
        return categories.Select(ItemResponse.FromItem).ToList();
    }

    public async Task<List<ItemResponse>> GetAllByListIdAsync(string userId, string listId)
    {
        var categories = await itemRepository.GetAllByListIdAsync(userId, listId);
        return categories.Select(ItemResponse.FromItem).ToList();
    }

    public async Task<ItemResponse?> CreateAsync(string userId, string listId, ItemRequest item)
    {
        var toCreate = item.ToItem(userId, listId);
        var result = await itemRepository.CreateAsync(toCreate);
        return result is not null
            ? ItemResponse.FromItem(result)
            : throw new ConflictException("Item cannot be created, it already exists");
    }

    public async Task<ItemResponse?> UpdateAsync(
        string userId,
        string listId,
        string itemId,
        ItemRequest requested
    )
    {
        var existing = await itemRepository.GetByIdAsync(userId, itemId);

        if (existing is null)
            throw new NotFoundException(
                $"Failed to update item {itemId} because it does not exist"
            );

        if (existing.ListId != listId)
            throw new BadRequestException(
                $"Failed to update item {itemId} because it does not belong to list {listId}"
            );

        var toUpdate = requested.ToItem(existing);
        var result = await itemRepository.UpdateAsync(toUpdate);

        if (result is not null)
        {
            return ItemResponse.FromItem(result);
        }

        throw new NotFoundException($"Failed to update item {itemId} even though it was found");
    }

    public async Task UpdateToDefaultCategoryAsync(
        string userId,
        string listId,
        string defaultCategoryId
    )
    {
        var items = await itemRepository.GetAllByListIdAsync(userId, listId);
        foreach (var item in items.Where(item => item.CategoryId != defaultCategoryId))
        {
            item.CategoryId = defaultCategoryId;
            await itemRepository.UpdateAsync(item);
        }
    }

    public async Task UpdateToDefaultCategoryAsync(
        string userId,
        string listId,
        string defaultCategoryId,
        string currentCategoryId
    )
    {
        var items = await itemRepository.GetAllByListIdAsync(userId, listId);
        foreach (var item in items.Where(item => item.CategoryId == currentCategoryId))
        {
            item.CategoryId = defaultCategoryId;
            await itemRepository.UpdateAsync(item);
        }
    }

    public async Task DeleteAllByListIdAsync(string userId, string listId)
    {
        var hasBeenDeleted = await itemRepository.DeleteAllByListIdAsync(userId, listId);
        if (!hasBeenDeleted)
        {
            throw new NotFoundException($"Failed to delete all items in list {listId}");
        }
    }

    public async Task DeleteByIdAsync(string userId, string listId, string itemId)
    {
        var hasBeenDeleted = await itemRepository.DeleteByIdAsync(userId, listId, itemId);
        if (!hasBeenDeleted)
        {
            throw new NotFoundException($"Failed to delete item {itemId} because it doesn't exist");
        }
    }
}
