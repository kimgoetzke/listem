using Listem.API.Contracts;
using Listem.API.Utilities;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Listem.API.Domain.ItemLists;

public class ItemListRequest
{
    public string Name { get; init; } = null!;
    public ListType ListType { get; init; } = ListType.Standard;

    public ItemList ToItemList(string userId)
    {
        return new ItemList
        {
            Id = IdProvider.NewId(nameof(ListType)),
            Name = Name,
            ListType = ListType,
            OwnerId = userId,
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };
    }

    public ItemList ToItemList(ItemList list)
    {
        return new ItemList
        {
            Id = list.Id,
            Name = Name,
            ListType = ListType,
            OwnerId = list.OwnerId,
            AddedOn = list.AddedOn,
            UpdatedOn = DateTime.Now
        };
    }

    public override string ToString()
    {
        return $"[ItemListRequest] '{Name}', type: {ListType}";
    }
}
