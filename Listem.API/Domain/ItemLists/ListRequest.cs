using Listem.API.Contracts;
using Listem.API.Utilities;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Listem.API.Domain.ItemLists;

public class ListRequest
{
    public string Name { get; init; } = null!;
    public ListType ListType { get; init; } = ListType.Standard;

    public List ToItemList(string userId)
    {
        return new List
        {
            Id = IdProvider.NewId(nameof(ListRequest)),
            Name = Name,
            ListType = ListType,
            OwnerId = userId,
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };
    }

    public List ToItemList(List list)
    {
        return new List
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
        return $"[ListRequest] '{Name}', type: {ListType}";
    }
}
