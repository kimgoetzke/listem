using Listem.Shared.Contracts;
using Listem.Shared.Utilities;

namespace Listem.API.Domain.Categories;

internal class Category : Entity
{
    public string Name { get; set; } = null!;
    public string ListId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public DateTime AddedOn { get; init; }
    public DateTime UpdatedOn { get; set; }

    public static Category From(CategoryRequest categoryRequest, string userId, string listId)
    {
        return new Category
        {
            Id = IdProvider.NewId(nameof(Category)),
            Name = categoryRequest.Name,
            ListId = listId,
            OwnerId = userId,
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };
    }

    public void Update(CategoryRequest categoryRequest)
    {
        Name = categoryRequest.Name;
        UpdatedOn = DateTime.Now;
    }

    public CategoryResponse ToResponse()
    {
        return new CategoryResponse
        {
            Id = Id,
            Name = Name,
            ListId = ListId,
            AddedOn = AddedOn,
            UpdatedOn = UpdatedOn
        };
    }

    public override string ToString()
    {
        return $"[Category] '{Name}' {Id} in {ListId}, owned by: {OwnerId}";
    }
}
