using Listem.API.Utilities;

namespace Listem.API.Domain.Categories;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

public class CategoryRequest
{
    public string Name { get; init; } = ICategoryService.DefaultCategoryName;

    public Category ToCategory(string userId, string listId)
    {
        return new Category
        {
            Id = IdProvider.NewId(nameof(Category)),
            Name = Name,
            ListId = listId,
            OwnerId = userId,
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };
    }

    public Category ToCategory(Category category)
    {
        return new Category
        {
            Id = category.Id,
            Name = Name,
            ListId = category.ListId,
            OwnerId = category.OwnerId,
            AddedOn = category.AddedOn,
            UpdatedOn = DateTime.Now
        };
    }

    public override string ToString()
    {
        return $"[CategoryRequest] '{Name}'";
    }
}
