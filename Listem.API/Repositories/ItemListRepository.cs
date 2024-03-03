using Listem.API.Contracts;
using Listem.API.Utilities;

namespace Listem.API.Repositories;

#pragma warning disable CS1998
public class ItemListRepository : IItemListRepository
{
    private readonly List<ItemList> _itemLists = [];

    public Task<List<ItemList>> GetAllAsync(string userId)
    {
        return Task.FromResult(_itemLists.FindAll(i => i.OwnerId == userId));
    }

    public Task<ItemList?> GetByIdAsync(string userId, string listId)
    {
        var itemList = _itemLists.FirstOrDefault(i => i.Id == listId && i.OwnerId == userId);
        Logger.Log($"Retrieved list: {itemList?.ToLoggableString() ?? "null"}");
        return Task.FromResult(itemList);
    }

    public async Task<ItemList?> CreateAsync(ItemList itemList)
    {
        _itemLists.Add(itemList);
        Logger.Log($"Added list: {itemList.ToLoggableString()}");
        // await CreateDefaultCategory(itemList.Id);
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
        Logger.Log($"Updated list: {existingList.ToLoggableString()}");
        return existingList;
    }

    public async Task<bool> DeleteAsync(ItemList itemList)
    {
        // TODO: Delete all categories and items associated with this list
        Logger.Log($"Removing list: '{itemList.Name}' {itemList.Id}");
        return _itemLists.Remove(itemList);
    }
#pragma warning restore CS1998
}
