using CommunityToolkit.Mvvm.ComponentModel;

namespace Listem.Models;

public partial class ObservableCategory(string listId) : ObservableObject
{
    [ObservableProperty]
    private string _id = "CAT~" + Guid.NewGuid().ToString().Replace("-", "");

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _listId = listId;

    public static ObservableCategory From(Category category)
    {
        return new ObservableCategory(category.ListId) { Id = category.Id, Name = category.Name };
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