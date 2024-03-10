using Listem.API.Utilities;
using Listem.Contracts;

namespace Listem.API.Domain.Items;

internal class Item : Entity
{
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public bool IsImportant { get; set; }
    public string CategoryId { get; set; } = null!;
    public string ListId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public DateTime AddedOn { get; init; }
    public DateTime UpdatedOn { get; set; }

    public static Item From(ItemRequest itemRequest, string userId, string listId)
    {
        return new Item
        {
            Id = IdProvider.NewId(nameof(Item)),
            Name = itemRequest.Name,
            Quantity = itemRequest.Quantity,
            IsImportant = itemRequest.IsImportant,
            CategoryId = itemRequest.CategoryId,
            ListId = listId,
            OwnerId = userId,
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };
    }

    public void Update(ItemRequest itemRequest)
    {
        Name = itemRequest.Name;
        Quantity = itemRequest.Quantity;
        IsImportant = itemRequest.IsImportant;
        CategoryId = itemRequest.CategoryId;
        UpdatedOn = DateTime.Now;
    }

    public ItemResponse ToResponse()
    {
        return new ItemResponse
        {
            Id = Id,
            Title = Name,
            Quantity = Quantity,
            IsImportant = IsImportant,
            ListId = ListId,
            CategoryId = CategoryId,
            AddedOn = AddedOn,
            UpdatedOn = UpdatedOn
        };
    }

    public override string ToString()
    {
        return $"[Item] '{Name}' {Id} in {ListId}, category: {CategoryId}, quantity: {Quantity}, important: {IsImportant}";
    }
}
