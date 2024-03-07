using Listem.API.Utilities;

namespace Listem.API.Domain.ItemLists;

#pragma warning disable CS1998
public class PlaceholderListRepository : IListRepository
{
    private readonly List<List> _itemLists = [];

    public Task<List<List>> GetAllAsync(string userId)
    {
        return Task.FromResult(_itemLists.FindAll(i => i.OwnerId == userId));
    }

    public Task<List?> GetByIdAsync(string userId, string listId)
    {
        var itemList = _itemLists.FirstOrDefault(i => i.Id == listId && i.OwnerId == userId);
        Logger.Log($"Retrieved list: {itemList?.ToString() ?? "null"}");
        return Task.FromResult(itemList);
    }

    public Task<bool> ExistsAsync(string userId, string listId)
    {
        return Task.FromResult(_itemLists.Exists(i => i.Id == listId && i.OwnerId == userId));
    }

    public async Task<List?> CreateAsync(List list)
    {
        _itemLists.Add(list);
        Logger.Log($"Added list: {list}");
        return _itemLists.FirstOrDefault(i => i.Id == list.Id);
    }

    public async Task<List?> UpdateAsync(List list)
    {
        var existingList = _itemLists.FirstOrDefault(i => i.Id == list.Id);

        if (existingList is null)
            return null;

        existingList.Name = list.Name;
        existingList.ListType = list.ListType;
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
