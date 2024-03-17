using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Shared.Contracts;
using Listem.Shared.Utilities;

namespace Listem.Mobile.Models;

public partial class ObservableItem(string listId) : ObservableObject
{
    [ObservableProperty]
    private string? _id;

    [ObservableProperty]
    private string _listId = listId;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private bool _isImportant;

    [ObservableProperty]
    private string _categoryName = Shared.Constants.DefaultCategoryName;

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

    public static ObservableItem From(ItemResponse item)
    {
        return new ObservableItem(item.ListId)
        {
            Id = item.Id,
            Title = item.Title,
            Quantity = item.Quantity,
            IsImportant = item.IsImportant,
            CategoryName = item.CategoryId,
            AddedOn = item.AddedOn
        };
    }

    public ItemRequest ToItemRequest()
    {
        return new ItemRequest
        {
            Name = Title,
            Quantity = Quantity,
            IsImportant = IsImportant,
            CategoryId = CategoryName,
        };
    }

    public Item ToItem()
    {
        return new Item
        {
            Id = IdProvider.NewId(nameof(Item)),
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
