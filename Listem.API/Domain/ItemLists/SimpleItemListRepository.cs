using Listem.API.Utilities;

namespace Listem.API.Domain.ItemLists;

#pragma warning disable CS1998
public class SimpleItemListRepository : IItemListRepository
{
    private readonly List<ItemList> _itemLists = [];

    public Task<List<ItemList>> GetAllAsync(string userId)
    {
        return Task.FromResult(_itemLists.FindAll(i => i.OwnerId == userId));
    }

    public Task<ItemList?> GetByIdAsync(string userId, string listId)
    {
        var itemList = _itemLists.FirstOrDefault(i => i.Id == listId && i.OwnerId == userId);
        Logger.Log($"Retrieved list: {itemList?.ToString() ?? "null"}");
        return Task.FromResult(itemList);
    }

    public Task<bool> ExistsAsync(string userId, string listId)
    {
        return Task.FromResult(_itemLists.Exists(i => i.Id == listId && i.OwnerId == userId));
    }

    public async Task<ItemList?> CreateAsync(ItemList itemList)
    {
        _itemLists.Add(itemList);
        Logger.Log($"Added list: {itemList}");
        return _itemLists.FirstOrDefault(i => i.Id == itemList.Id);
    }

    public async Task<ItemList?> UpdateAsync(ItemList itemList)
    {
        var existingList = _itemLists.FirstOrDefault(i => i.Id == itemList.Id);

        if (existingList is null)
            return null;

        existingList.Name = itemList.Name;
        existingList.ListType = itemList.ListType;
        existingList.UpdatedOn = DateTime.Now;
        Logger.Log($"Updated list: {existingList}");
        return existingList;
    }

    public async Task<bool> DeleteByIdAsync(string userId, string listId)
    {
        Logger.Log($"Removing list: {listId} by {userId}");
        return _itemLists.RemoveAll(i => i.Id == listId && i.OwnerId == userId) > 0;
    }
#pragma warning restore CS1998
}
