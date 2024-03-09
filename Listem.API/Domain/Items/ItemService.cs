using Listem.API.Contracts;
using Listem.API.Exceptions;

namespace Listem.API.Domain.Items;

internal class ItemService(IItemRepository itemRepository, ILogger<ItemService> logger)
    : IItemService
{
    public async Task<List<ItemResponse>> GetAllAsync()
    {
        var result = await itemRepository.GetAllAsync();
        return result.Select(i => i.ToResponse()).ToList();
    }

    public async Task<List<ItemResponse>> GetAllByListIdAsync(string listId)
    {
        var result = await itemRepository.GetAllByListIdAsync(listId);
        return result.Select(i => i.ToResponse()).ToList();
    }

    public async Task<ItemResponse?> CreateAsync(string userId, string listId, ItemRequest item)
    {
        var toCreate = Item.From(item, userId, listId);
        var result = await itemRepository.CreateAsync(toCreate);
        return result is not null
            ? result.ToResponse()
            : throw new ConflictException("Item cannot be created, it already exists");
    }

    public async Task<ItemResponse?> UpdateAsync(
        string listId,
        string itemId,
        ItemRequest itemRequest
    )
    {
        var existing = await itemRepository.GetByIdAsync(itemId);

        if (existing is null)
            throw new NotFoundException(
                $"Failed to update item {itemId} because it does not exist"
            );

        if (existing.ListId != listId)
            throw new BadRequestException(
                $"Failed to update item {itemId} because it does not belong to list {listId}"
            );

        existing.Update(itemRequest);
        var result = await itemRepository.UpdateAsync(existing);

        if (result is not null)
        {
            return result.ToResponse();
        }

        throw new NotFoundException($"Failed to update item {itemId} even though it was found");
    }

    public async Task UpdateToDefaultCategoryAsync(string listId, string defaultCategoryId)
    {
        var items = await itemRepository.GetAllByListIdAsync(listId);
        foreach (var item in items.Where(item => item.CategoryId != defaultCategoryId))
        {
            item.CategoryId = defaultCategoryId;
            await itemRepository.UpdateAsync(item);
        }
    }

    public async Task UpdateToDefaultCategoryAsync(
        string listId,
        string defaultCategoryId,
        string currentCategoryId
    )
    {
        var items = await itemRepository.GetAllByListIdAsync(listId);
        foreach (var item in items.Where(item => item.CategoryId == currentCategoryId))
        {
            item.CategoryId = defaultCategoryId;
            await itemRepository.UpdateAsync(item);
        }
    }

    public async Task DeleteAllByListIdAsync(string listId)
    {
        var hasBeenDeleted = await itemRepository.DeleteAllByListIdAsync(listId);
        if (!hasBeenDeleted)
        {
            logger.LogInformation("There are no items to delete in list {ListId}", listId);
        }
    }

    public async Task DeleteByIdAsync(string listId, string itemId)
    {
        var hasBeenDeleted = await itemRepository.DeleteByIdAsync(listId, itemId);
        if (!hasBeenDeleted)
        {
            throw new NotFoundException($"Failed to delete item {itemId} because it doesn't exist");
        }
    }
}
