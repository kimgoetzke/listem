namespace Listem.Shared.Contracts;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

public class CategoryRequest
{
    public string Name { get; init; } = Constants.DefaultCategoryName;

    public override string ToString()
    {
        return $"[CategoryRequest] '{Name}'";
    }
}
