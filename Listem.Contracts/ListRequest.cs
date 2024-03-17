// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Listem.Contracts;

public class ListRequest
{
    public string Name { get; init; } = null!;
    public ListType ListType { get; init; } = ListType.Standard;

    public override string ToString()
    {
        return $"[ListRequest] '{Name}', type: {ListType}";
    }
}
