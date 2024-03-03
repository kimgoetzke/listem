namespace Listem.API.Contracts;

public class ItemListRequest
{
    public string Name { get; init; } = null!;
    public ListType ListType { get; init; } = ListType.Standard;

    public ItemList ToItemList(string userId)
    {
        var id = "LST~" + Guid.NewGuid().ToString().Replace("-", "");
        return new ItemList
        {
            Id = id,
            Name = Name,
            ListType = ListType,
            OwnerId = userId,
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };
    }

    public ItemList ToItemList(ItemList existingList)
    {
        return new ItemList
        {
            Id = existingList.Id,
            Name = Name,
            ListType = ListType,
            OwnerId = existingList.OwnerId,
            AddedOn = existingList.AddedOn,
            UpdatedOn = DateTime.Now
        };
    }
}
