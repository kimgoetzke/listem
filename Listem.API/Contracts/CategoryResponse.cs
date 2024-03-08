using Listem.API.Domain.Categories;

namespace Listem.API.Contracts;

public class CategoryResponse
{
    public string Id { get; private init; } = null!;
    public string Name { get; private init; } = null!;
    public string ListId { get; private init; } = null!;
    public DateTime AddedOn { get; private init; }
    public DateTime UpdatedOn { get; private init; }

    public static CategoryResponse FromCategory(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ListId = category.ListId,
            AddedOn = category.AddedOn,
            UpdatedOn = category.UpdatedOn
        };
    }

    public override string ToString()
    {
        return $"[CategoryResponse] '{Name}' {Id} in {ListId}";
    }
}
