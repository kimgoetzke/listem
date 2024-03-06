using Listem.API.Contracts;

namespace Listem.API.Domain.ItemLists;

public class ItemListResponse
{
    public string Id { get; private init; } = null!;
    public string Name { get; private init; } = null!;
    public ListType ListType { get; private init; } = ListType.Standard;
    public DateTime AddedOn { get; private init; }
    public DateTime UpdatedOn { get; private init; }

    public static ItemListResponse FromItemList(ItemList list)
    {
        return new ItemListResponse
        {
            Id = list.Id,
            Name = list.Name,
            ListType = list.ListType,
            AddedOn = list.AddedOn,
            UpdatedOn = list.UpdatedOn
        };
    }

    public override string ToString()
    {
        return $"[ItemListResponse] '{Name}' {Id}, type: {ListType}";
    }
}
