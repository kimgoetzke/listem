using Listem.API.Contracts;
using Listem.API.Repositories;
using Listem.API.Utilities;

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
        return fetched is not null ? ItemListResponse.FromItemList(fetched) : null;
    }

    public async Task<ItemListResponse?> CreateAsync(string userId, ItemListRequest itemListRequest)
    {
        var toCreate = itemListRequest.ToItemList(userId);
        var result = await itemListRepository.CreateAsync(toCreate);
        return ItemListResponse.FromItemList(result!);
    }

    public async Task<ItemListResponse?> UpdateAsync(
        string userId,
        string id,
        ItemListRequest requested
    )
    {
        var existing = await itemListRepository.GetByIdAsync(userId, id);

        if (existing is not null)
        {
            var toUpdate = requested.ToItemList(existing);
            var result = await itemListRepository.UpdateAsync(toUpdate);

            if (result is not null)
                return ItemListResponse.FromItemList(result);

            Logger.Log($"Failed to update list with {id} even though it was found");
            return null;
        }

        Logger.Log($"Failed to update list with {id} because it does not exist");
        return null;
    }

    public async Task<bool> DeleteAsync(string userId, string id)
    {
        var existing = await itemListRepository.GetByIdAsync(userId, id);
        if (existing is null)
        {
            Logger.Log($"Failed to delete list with {id} because it does not exist");
            return false;
        }

        var hasSucceeded = await itemListRepository.DeleteAsync(existing);
        if (!hasSucceeded)
        {
            Logger.Log($"Failed to delete list with id {id} even though it was found");
        }
        return hasSucceeded;
    }
}
