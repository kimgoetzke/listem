using System.Linq.Expressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Listem.Mobile.Views;
using Realms;
using StringProcessor = Listem.Mobile.Utilities.StringProcessor;

namespace Listem.Mobile.ViewModel;

public partial class ListViewModel : ObservableObject
{
    [ObservableProperty]
    private List _currentList;

    [ObservableProperty]
    private IQueryable<Item> _items = null!;

    [ObservableProperty]
    private List<Item> _itemsToDelete;

    [ObservableProperty]
    private IList<Category> _categories;

    [ObservableProperty]
    private string _newItemName = string.Empty;

    [ObservableProperty]
    private int _newItemQuantity = 1;

    [ObservableProperty]
    private bool _newItemIsImportant;

    [ObservableProperty]
    private Category _currentCategory;

    private readonly IItemService _itemService;
    private readonly IClipboardService _clipboardService;
    private readonly Realm _realm = RealmService.GetMainThreadRealm();

    public ListViewModel(List list, IServiceProvider serviceProvider)
    {
        _itemService = serviceProvider.GetService<IItemService>()!;
        _clipboardService = serviceProvider.GetService<IClipboardService>()!;
        CurrentList = list;
        ItemsToDelete = [];
        Categories = CurrentList.Categories;
        CurrentCategory = Categories.First(c => c.Name == Shared.Constants.DefaultCategoryName);
        GetSortedItems();
    }

    [RelayCommand]
    private async Task AddItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemName))
            return;

        var newItem = new Item
        {
            Name = StringProcessor.TrimAndCapitalise(NewItemName),
            OwnedBy = RealmService.User.Id!,
            List = CurrentList,
            Category = new Category { Name = CurrentCategory.Name },
            Quantity = NewItemQuantity,
            IsImportant = NewItemIsImportant,
            UpdatedOn = DateTime.Now.ToUniversalTime(),
        };

        await _itemService.CreateAsync(newItem);
        Notifier.ShowToast($"Added: {newItem.Name}");

        NewItemName = string.Empty;
        NewItemQuantity = 1;
        NewItemIsImportant = false;
        GetSortedItems();
    }

    [RelayCommand]
    private async Task RemoveItem(Item item)
    {
        await _itemService.DeleteAsync(item);
    }

    [RelayCommand]
    private async Task TogglePriority(Item item)
    {
        await _itemService.UpdateAsync(item, isImportant: !item.IsImportant);
        GetSortedItems();
    }

    [RelayCommand]
    private static async Task GoBack()
    {
        await Shell.Current.Navigation.PopAsync();
    }

    [RelayCommand]
    private static async Task TapItem(Item item)
    {
        await Shell.Current.Navigation.PushModalAsync(new DetailPage(item));
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        _clipboardService.CopyToClipboard(Items.ToList(), Categories.ToList());
    }

    [RelayCommand]
    private void InsertFromClipboard()
    {
        _clipboardService.InsertFromClipboardAsync(
            Items.ToList(),
            Categories.ToList(),
            CurrentList
        );
    }

    // TODO: Test this because it'll probably break when using Realm
    public string ProcessedObservableItemListName
    {
        get
        {
            if (CurrentList.Name.Length > 15)
            {
                return CurrentList.Name[..15].Trim() + "...";
            }

            return CurrentList.Name;
        }
    }

    // TODO: Implement custom sorting and filtering using locally stored preferences
    // Note that listing default category items first or last may be difficult, see:
    // https://www.mongodb.com/community/forums/t/c-linq-condition-inside-orderby/149393
    public void GetSortedItems()
    {
        Items = _realm
            .All<Item>()
            .Where(i => i.List == CurrentList)
            .OrderBy(i => i.Category!.Name)
            .ThenByDescending(i => i.UpdatedOn);
    }
}
