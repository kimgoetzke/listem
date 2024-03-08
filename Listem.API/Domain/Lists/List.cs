using Listem.API.Contracts;

namespace Listem.API.Domain.Lists;

public class List
{
    public string Id { get; init; } = null!;
    public string Name { get; set; } = null!;
    public ListType ListType { get; set; }
    public string OwnerId { get; init; } = null!;
    public DateTime AddedOn { get; init; }
    public DateTime UpdatedOn { get; set; }

    public override string ToString()
    {
        return $"[List] '{Name}' {Id}, type: '{ListType.ToString()}', last updated: {UpdatedOn}";
    }
}
