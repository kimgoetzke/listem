using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Shared.Contracts;
using Listem.Shared.Utilities;

namespace Listem.Mobile.Models;

public partial class ObservableCategory(string listId) : ObservableObject
{
    [ObservableProperty]
    private string? _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _listId = listId;

    public static ObservableCategory From(Category category)
    {
        return new ObservableCategory(category.ListId) { Id = category.Id, Name = category.Name };
    }

    public static ObservableCategory From(CategoryResponse category)
    {
        return new ObservableCategory(category.ListId) { Id = category.Id, Name = category.Name };
    }

    public CategoryRequest ToCategoryRequest()
    {
        return new CategoryRequest { Name = Name };
    }

    public Category ToCategory()
    {
        return new Category
        {
            Id = IdProvider.NewId(nameof(Category)),
            Name = Name,
            ListId = ListId
        };
    }

    public override string ToString()
    {
        return Name;
    }

    public string ToLoggableString()
    {
        return $"[ObservableCategory] {Name} ({Id}) in {ListId}";
    }
}
