using Listem.API.Exceptions;

namespace Listem.API.Domain.ItemLists;

public class ItemListService(IItemListRepository itemListRepository) : IItemListService
{
    public async Task<List<ItemListResponse>> GetAllAsync(string userId)
    {
        var itemLists = await itemListRepository.GetAllAsync(userId);
        return itemLists.Select(ItemListResponse.FromItemList).ToList();
    }

    public async Task<ItemListResponse?> GetByIdAsync(string userId, string listId)
    {
        var fetched = await itemListRepository.GetByIdAsync(userId, listId);
        return fetched is not null
            ? ItemListResponse.FromItemList(fetched)
            : throw new NotFoundException("List not found");
    }

    public Task<bool> ExistsAsync(string userId, string listId)
    {
        return itemListRepository.ExistsAsync(userId, listId);
    }

    public async Task<ItemListResponse?> CreateAsync(string userId, ItemListRequest itemListRequest)
    {
        var toCreate = itemListRequest.ToItemList(userId);
        var result = await itemListRepository.CreateAsync(toCreate);
        // await CreateDefaultCategory(itemList.Id);
        return result is not null
            ? ItemListResponse.FromItemList(result)
            : throw new ConflictException("List cannot be created, it already exists");
    }

    public async Task<ItemListResponse?> UpdateAsync(
        string userId,
        string listId,
        ItemListRequest requested
    )
    {
        var existing = await itemListRepository.GetByIdAsync(userId, listId);

        if (existing is null)
            throw new NotFoundException(
                $"Failed to update list {listId} because it does not exist"
            );

        var toUpdate = requested.ToItemList(existing);
        var result = await itemListRepository.UpdateAsync(toUpdate);

        if (result is not null)
            return ItemListResponse.FromItemList(result);

        throw new NotFoundException($"Failed to update list {listId} even though it was found");
    }

    public async Task DeleteByIdAsync(string userId, string listId)
    {
        var hasBeenDeleted = await itemListRepository.DeleteByIdAsync(userId, listId);
        if (!hasBeenDeleted)
        {
            throw new NotFoundException($"Failed to delete list {listId}");
        }
    }
}
