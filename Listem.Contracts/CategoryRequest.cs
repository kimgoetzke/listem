namespace Listem.Contracts;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

public class CategoryRequest
{
    public string Name { get; init; } = CategoryDefault.Name;

    public override string ToString()
    {
        return $"[CategoryRequest] '{Name}'";
    }
}
