// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

using Listem.Shared.Enums;

namespace Listem.Shared.Contracts;

public class ListRequest
{
    public string Name { get; init; } = null!;
    public ListType ListType { get; init; } = ListType.Standard;

    public override string ToString()
    {
        return $"[ListRequest] '{Name}', type: {ListType}";
    }
}
