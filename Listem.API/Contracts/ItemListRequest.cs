using Listem.Contracts;

namespace Listem.API.Contracts;

public class ItemListRequest
{
    public string Name { get; init; } = null!;
    public ListType ListType { get; init; } = ListType.Standard;
    public DateTime AddedOn { get; init; } = DateTime.Now;
    public DateTime UpdatedOn { get; init; } = DateTime.Now;

    public ItemList ToItemList()
    {
        var id = "LST~" + Guid.NewGuid().ToString().Replace("-", "");
        return ToItemList(id);
    }

    public ItemList ToItemList(string id)
    {
        return new ItemList
        {
            Id = id,
            Name = Name,
            ListType = ListType,
            AddedOn = AddedOn,
            UpdatedOn = UpdatedOn
        };
    }
}
