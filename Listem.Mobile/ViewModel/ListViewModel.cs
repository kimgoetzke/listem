using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
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
    private List _list;

    [ObservableProperty]
    private Item _newItem;

    [ObservableProperty]
    private IQueryable<Item> _items;

    [ObservableProperty]
    private List<Item> _itemsToDelete;

    [ObservableProperty]
    private IList<Category> _categories;

    [ObservableProperty]
    private Category? _currentCategory;

    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly IClipboardService _clipboardService;
    private readonly Realm _realm = RealmService.GetMainThreadRealm();

    public ListViewModel(List list, IServiceProvider serviceProvider)
    {
        _categoryService = serviceProvider.GetService<ICategoryService>()!;
        _itemService = serviceProvider.GetService<IItemService>()!;
        _clipboardService = serviceProvider.GetService<IClipboardService>()!;
        List = list;
        ItemsToDelete = [];
        Categories = List.Categories;
        CurrentCategory = Categories.First(c => c.Name == Shared.Constants.DefaultCategoryName);
        Items = _realm.All<Item>().Where(i => i.List!.Id == list.Id);
        NewItem = new Item { List = list };
        SortItems();
    }

    [RelayCommand]
    private async Task AddItem()
    {
        if (string.IsNullOrWhiteSpace(NewItem.Name))
            return;

        NewItem.Name = StringProcessor.TrimAndCapitalise(NewItem.Name);
        NewItem.UpdatedOn = DateTime.Now.ToUniversalTime();
        await _itemService.CreateOrUpdateAsync(NewItem);
        var value = new ItemChangedDto(List.Id, NewItem);
        WeakReferenceMessenger.Default.Send(new ItemAddedToListMessage(value));
        Notifier.ShowToast($"Added: {NewItem.Name}");

        NewItem = new Item() { List = List, Category = CurrentCategory };
        SortItems();
    }

    [RelayCommand]
    private async Task RemoveItem(Item i)
    {
        await _itemService.DeleteAsync(i);
        var value = new ItemChangedDto(List.Id, i);
        WeakReferenceMessenger.Default.Send(new ItemRemovedFromListMessage(value));
        OnPropertyChanged(nameof(List));
    }

    [RelayCommand]
    private async Task RemoveAllItems()
    {
        if (!await IsRequestConfirmedByUser())
            return;

        await _itemService.DeleteAllByListIdAsync(List.Id);
        Notifier.ShowToast("Removed all items from list");
    }

    private static async Task<bool> IsRequestConfirmedByUser()
    {
        return await Shell.Current.DisplayAlert(
            "Clear list",
            $"This will remove all items from your list. Are you sure you want to continue?",
            "Yes",
            "No"
        );
    }

    [RelayCommand]
    private async Task TogglePriority(Item i)
    {
        i.IsImportant = !i.IsImportant;
        await _itemService.CreateOrUpdateAsync(i);
        SortItems();
    }

    [RelayCommand]
    private static async Task GoBack(Item i)
    {
        await Shell.Current.Navigation.PopAsync();
    }

    [RelayCommand]
    private async Task TapItem(Item i)
    {
        await Shell.Current.Navigation.PushModalAsync(new DetailPage(i, List));
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        _clipboardService.CopyToClipboard(Items.ToList(), Categories.ToList());
    }

    [RelayCommand]
    private void InsertFromClipboard()
    {
        _clipboardService.InsertFromClipboardAsync(Items.ToList(), Categories.ToList(), List);
    }

    public string ProcessedObservableItemListName
    {
        get
        {
            if (List.Name.Length > 15)
            {
                return List.Name[..15].Trim() + "...";
            }

            return List.Name;
        }
    }

    private void SortItems()
    {
        OnPropertyChanged(nameof(Items));
    }
}
