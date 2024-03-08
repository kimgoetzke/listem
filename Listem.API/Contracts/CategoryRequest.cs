using Listem.API.Domain.Categories;

namespace Listem.API.Contracts;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

public class CategoryRequest
{
    public string Name { get; init; } = ICategoryService.DefaultCategoryName;

    public override string ToString()
    {
        return $"[CategoryRequest] '{Name}'";
    }
}
