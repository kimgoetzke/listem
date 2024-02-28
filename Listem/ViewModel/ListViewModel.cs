using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Events;
using Listem.Models;
using Listem.Services;
using Listem.Utilities;
using Listem.Views;
using StringProcessor = Listem.Utilities.StringProcessor;

namespace Listem.ViewModel;

public partial class ListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableItemList _observableItemList;

    [ObservableProperty]
    private ObservableItem _newObservableItem;

    [ObservableProperty]
    private ObservableCollection<ObservableItem> _items = [];

    [ObservableProperty]
    private ObservableCollection<ObservableItem> _itemsToDelete = [];

    [ObservableProperty]
    private ObservableCollection<ObservableCategory> _categories = [];

    [ObservableProperty]
    private ObservableCategory? _currentCategory;

    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly IClipboardService _clipboardService;

    public ListViewModel(ObservableItemList observableItemList)
    {
        _categoryService = IPlatformApplication.Current!.Services.GetService<ICategoryService>()!;
        _itemService = IPlatformApplication.Current.Services.GetService<IItemService>()!;
        _clipboardService = IPlatformApplication.Current.Services.GetService<IClipboardService>()!;
        ObservableItemList = observableItemList;
        Items = new ObservableCollection<ObservableItem>(observableItemList.Items);
        NewObservableItem = new ObservableItem(ObservableItemList.Id);
        SortItems();
        LoadCategories().SafeFireAndForget();
    }

    private async Task LoadCategories()
    {
        var categories = await _categoryService.GetAllByListIdAsync(ObservableItemList.Id);
        Categories = new ObservableCollection<ObservableCategory>(categories);
        CurrentCategory = Categories.FirstOrDefault(c =>
            c.Name == ICategoryService.DefaultCategoryName
        );
        OnPropertyChanged(nameof(Categories));
        OnPropertyChanged(nameof(CurrentCategory));
        Logger.Log(
            $"Loaded {Categories.Count} categories for list {ObservableItemList.Id} from the database"
        );
    }

    [RelayCommand]
    private async Task AddItem()
    {
        // Don't add empty items
        if (string.IsNullOrWhiteSpace(NewObservableItem.Title))
            return;

        // Pre-process item
        NewObservableItem.Title = StringProcessor.TrimAndCapitalise(NewObservableItem.Title);
        NewObservableItem.CategoryName =
            CurrentCategory != null ? CurrentCategory.Name : ICategoryService.DefaultCategoryName;

        // Add to list and database
        Logger.Log($"Adding item: {NewObservableItem.ToLoggableString()}");
        var value = new ItemChangedDto(ObservableItemList.Id, NewObservableItem);
        WeakReferenceMessenger.Default.Send(new ItemAddedToListMessage(value));
        await _itemService.CreateOrUpdateAsync(NewObservableItem);
        Items.Add(NewObservableItem);
        Notifier.ShowToast($"Added: {NewObservableItem.Title}");

        // Make sure the UI is reset/updated
        NewObservableItem = new ObservableItem(ObservableItemList.Id);
        SortItems();
        OnPropertyChanged(nameof(NewObservableItem));
    }

    [RelayCommand]
    private async Task RemoveItem(ObservableItem i)
    {
        await _itemService.DeleteAsync(i);
        ObservableItemList.Items.Remove(i);
        Items.Remove(i);
        var value = new ItemChangedDto(ObservableItemList.Id, i);
        WeakReferenceMessenger.Default.Send(new ItemRemovedFromListMessage(value));
        OnPropertyChanged(nameof(ObservableItemList));
    }

    [RelayCommand]
    private async Task RemoveAllItems()
    {
        if (!await IsRequestConfirmedByUser())
            return;

        Items.Clear();
        await _itemService.DeleteAllByListIdAsync(ObservableItemList.Id);
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
    private async Task TogglePriority(ObservableItem i)
    {
        i.IsImportant = !i.IsImportant;
        await _itemService.CreateOrUpdateAsync(i);
        SortItems();
    }

    [RelayCommand]
    private static async Task GoBack(ObservableItem i)
    {
        await Shell.Current.Navigation.PopAsync();
    }

    [RelayCommand]
    private async Task TapItem(ObservableItem i)
    {
        await Shell.Current.Navigation.PushModalAsync(new DetailPage(i, ObservableItemList));
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        _clipboardService.CopyToClipboard(Items, Categories);
    }

    [RelayCommand]
    private void InsertFromClipboard()
    {
        _clipboardService.InsertFromClipboardAsync(Items, Categories, ObservableItemList.Id);
    }

    public string ProcessedObservableItemListName
    {
        get
        {
            if (ObservableItemList.Name.Length > 15)
            {
                return ObservableItemList.Name[..15].Trim() + "...";
            }

            return ObservableItemList.Name;
        }
    }

    private void SortItems()
    {
        Items = new ObservableCollection<ObservableItem>(
            Items.OrderBy(i => i.CategoryName).ThenByDescending(i => i.AddedOn)
        );
        OnPropertyChanged(nameof(Items));
    }
}
