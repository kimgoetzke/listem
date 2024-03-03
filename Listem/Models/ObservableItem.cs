using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Services;

namespace Listem.Models;

public partial class ObservableItem(string listId) : ObservableObject
{
    [ObservableProperty]
    private string _id = "ITM~" + Guid.NewGuid().ToString().Replace("-", "");

    [ObservableProperty]
    private string _listId = listId;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private bool _isImportant;

    [ObservableProperty]
    private string _categoryName = ICategoryService.DefaultCategoryName;

    [ObservableProperty]
    private DateTime _addedOn = DateTime.Now;

    public static ObservableItem From(Item item)
    {
        return new ObservableItem(item.ListId)
        {
            Id = item.Id,
            Title = item.Title,
            Quantity = item.Quantity,
            IsImportant = item.IsImportant,
            CategoryName = item.CategoryName,
            AddedOn = item.AddedOn
        };
    }

    public Item ToItem()
    {
        return new Item
        {
            Id = Id,
            ListId = ListId,
            Title = Title,
            Quantity = Quantity,
            IsImportant = IsImportant,
            CategoryName = CategoryName,
            AddedOn = AddedOn
        };
    }

    public override string ToString()
    {
        return Title;
    }

    public string ToLoggableString()
    {
        return $"[ObservableItem] {Title} {Id} in {ListId} (category: {CategoryName}, quantity: {Quantity}, important: {IsImportant})";
    }
}
