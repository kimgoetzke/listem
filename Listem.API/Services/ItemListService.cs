using Listem.API.Contracts;
using Listem.API.Repositories;
using Listem.API.Utilities;
using Listem.Contracts;

namespace Listem.API.Services;

public class ItemListService(IItemListRepository itemListRepository)
{
    public async Task<List<ItemList>> GetAllAsync()
    {
        return await itemListRepository.GetAllAsync();
    }

    public async Task<ItemList?> GetByIdAsync(string id)
    {
        return await itemListRepository.GetByIdAsync(id);
    }

    public async Task<ItemList?> CreateAsync(ItemListRequest itemListRequest)
    {
        var itemList = itemListRequest.ToItemList();
        return await itemListRepository.CreateAsync(itemList);
    }

    public async Task<ItemList?> UpdateAsync(string id, ItemListRequest itemListRequest)
    {
        var existingList = await itemListRepository.GetByIdAsync(id);

        if (existingList is not null)
        {
            var itemList = itemListRequest.ToItemList(id);
            return await itemListRepository.UpdateAsync(itemList);
        }

        Logger.Log($"Failed to update list because it does not exist: {id}");
        return null;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var itemList = await itemListRepository.GetByIdAsync(id);
        if (itemList is null)
        {
            Logger.Log($"Failed to delete list with id {id} because it does not exist");
            return false;
        }
        var hasSucceeded = await itemListRepository.DeleteAsync(itemList);
        if (!hasSucceeded)
        {
            Logger.Log($"Failed to delete list with id {id} even though it was found");
        }
        return hasSucceeded;
    }
}
