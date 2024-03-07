using Listem.API.Utilities;

namespace Listem.API.Domain.Items;

#pragma warning disable CS1998
public class PlaceholderItemRepository : IItemRepository
{
    private readonly List<Item> _items = [];

    public Task<List<Item>> GetAllAsync(string userId)
    {
        var items= _items.FindAll(i => i.OwnerId == userId);
        Logger.Log($"Retrieved {items.Count} items");
        return Task.FromResult(items);
    }

    public Task<List<Item>> GetAllByListIdAsync(string userId, string listId)
    {
        var items = _items.FindAll(i => i.Id == listId && i.OwnerId == userId);
        Logger.Log($"Retrieved {items.Count} items for list {listId}");
        return Task.FromResult(items);
    }
    
    public Task<Item?> GetByIdAsync(string userId, string itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId && i.OwnerId == userId);
        Logger.Log($"Retrieved item: {item?.ToString() ?? "null"}");
        return Task.FromResult(item);
    }

    public async Task<Item?> CreateAsync(Item item)
    {
        _items.Add(item);
        Logger.Log($"Added item: {item}");
        return _items.FirstOrDefault(i => i.Id == item.Id);
    }

    public async Task<Item?> UpdateAsync(Item item)
    {
        var existingItem = _items.FirstOrDefault(i => i.Id == item.Id);

        if (existingItem is null)
            return null;

        existingItem.Name = item.Name;
        existingItem.IsImportant = item.IsImportant;
        existingItem.CategoryId = item.CategoryId;
        existingItem.Quantity = item.Quantity;
        existingItem.UpdatedOn = DateTime.Now;
        Logger.Log($"Updated item: {existingItem}");
        return existingItem;
    }
    
    public async Task<bool> DeleteAllByListIdAsync(string userId, string listId)
    {
        Logger.Log($"Removing all items in list {listId} by {userId}");
        return _items.RemoveAll(i => i.ListId == listId && i.OwnerId == userId) > 0;
    }

    public async Task<bool> DeleteByIdAsync(string userId, string itemId)
    {
        Logger.Log($"Removing item: {itemId} by {userId}");
        return _items.RemoveAll(i => i.Id == itemId && i.OwnerId == userId) > 0;
    }
#pragma warning restore CS1998
}
