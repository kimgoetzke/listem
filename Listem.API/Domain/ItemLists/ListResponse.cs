using Listem.API.Contracts;

namespace Listem.API.Domain.ItemLists;

public class ListResponse
{
    public string Id { get; private init; } = null!;
    public string Name { get; private init; } = null!;
    public ListType ListType { get; private init; } = ListType.Standard;
    public DateTime AddedOn { get; private init; }
    public DateTime UpdatedOn { get; private init; }

    public static ListResponse FromItemList(List list)
    {
        return new ListResponse
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
        return $"[ListResponse] '{Name}' {Id}, type: {ListType}";
    }
}
