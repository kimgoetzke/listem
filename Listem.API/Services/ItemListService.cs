using Listem.API.Contracts;
using Listem.API.Exceptions;
using Listem.API.Repositories;

namespace Listem.API.Services;

public class ItemListService(IItemListRepository itemListRepository)
{
    public async Task<List<ItemListResponse>> GetAllAsync(string userId)
    {
        var itemLists = await itemListRepository.GetAllAsync(userId);
        return itemLists.Select(ItemListResponse.FromItemList).ToList();
    }

    public async Task<ItemListResponse?> GetByIdAsync(string userId, string id)
    {
        var fetched = await itemListRepository.GetByIdAsync(userId, id);
        return fetched is not null
            ? ItemListResponse.FromItemList(fetched)
            : throw new NotFoundException("List not found");
    }

    public async Task<ItemListResponse?> CreateAsync(string userId, ItemListRequest itemListRequest)
    {
        var toCreate = itemListRequest.ToItemList(userId);
        var result = await itemListRepository.CreateAsync(toCreate);
        return result is not null
            ? ItemListResponse.FromItemList(result)
            : throw new ConflictException("List cannot be created, it already exists");
    }

    public async Task<ItemListResponse?> UpdateAsync(
        string userId,
        string id,
        ItemListRequest requested
    )
    {
        var existing = await itemListRepository.GetByIdAsync(userId, id);

        if (existing is null)
            throw new NotFoundException($"Failed to update list {id} because it does not exist");

        var toUpdate = requested.ToItemList(existing);
        var result = await itemListRepository.UpdateAsync(toUpdate);

        if (result is not null)
            return ItemListResponse.FromItemList(result);

        throw new NotFoundException($"Failed to update list {id} even though it was found");
    }

    public async Task DeleteByIdAsync(string userId, string id)
    {
        var hasBeenDeleted = await itemListRepository.DeleteByIdAsync(userId, id);
        if (!hasBeenDeleted)
        {
            throw new NotFoundException($"Failed to delete list {id}");
        }
    }
}
