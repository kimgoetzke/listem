namespace Listem.API.Contracts;

public class ItemListResponse
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public ListType ListType { get; init; } = ListType.Standard;
    public DateTime AddedOn { get; init; }
    public DateTime UpdatedOn { get; init; }

    public static ItemListResponse FromItemList(ItemList existingList)
    {
        return new ItemListResponse
        {
            Id = existingList.Id,
            Name = existingList.Name,
            ListType = existingList.ListType,
            AddedOn = existingList.AddedOn,
            UpdatedOn = existingList.UpdatedOn
        };
    }
}
