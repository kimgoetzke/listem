using Listem.Shared.Enums;

namespace Listem.Shared.Contracts;

public class ListResponse : Entity
{
    public string Name { get; init; } = null!;
    public ListType ListType { get; init; } = ListType.Standard;
    public DateTime AddedOn { get; init; }
    public DateTime UpdatedOn { get; init; }

    public override string ToString()
    {
        return $"[ListResponse] '{Name}' {Id}, type: {ListType}";
    }
}
