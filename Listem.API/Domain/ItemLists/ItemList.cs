using Listem.API.Contracts;

namespace Listem.API.Domain.ItemLists;

public class ItemList
{
    public string Id { get; init; } = null!;
    public string Name { get; set; } = null!;
    public string OwnerId { get; init; } = null!;
    public ListType ListType { get; set; }
    public DateTime AddedOn { get; init; }
    public DateTime UpdatedOn { get; set; }

    public override string ToString()
    {
        return $"[ItemList] '{Name}' {Id}, type: '{ListType.ToString()}', last updated: {UpdatedOn}";
    }
}
