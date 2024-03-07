using Listem.API.Utilities;

namespace Listem.API.Domain.Items;

public class ItemRequest
{
    public string Name { get; set; } = null!;
    public int Quantity { get; init; }
    public bool IsImportant { get; init; }
    public string CategoryId { get; set; } = null!;
    
    public Item ToItem(string userId, string listId)
    {
        return new Item
        {
            Id = IdProvider.NewId(nameof(Item)),
            Name = Name,
            Quantity = Quantity,
            IsImportant = IsImportant,
            CategoryId = CategoryId,
            ListId = listId,
            OwnerId = userId,
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };
    }
    
    public Item ToItem(Item item)
    {
        return new Item
        {
            Id = item.Id,
            Name = Name,
            Quantity = Quantity,
            IsImportant = IsImportant,
            CategoryId = CategoryId,
            ListId = item.ListId,
            OwnerId = item.OwnerId,
            AddedOn = item.AddedOn,
            UpdatedOn = DateTime.Now
        };
    }

    public override string ToString()
    {
        return $"[ItemRequest] '{Name}', quantity: {Quantity}, important: {IsImportant}, category: {CategoryId}";
    }
}
