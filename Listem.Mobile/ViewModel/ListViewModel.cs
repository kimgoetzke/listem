using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Listem.Mobile.Views;
using StringProcessor = Listem.Mobile.Utilities.StringProcessor;

namespace Listem.Mobile.ViewModel;

public partial class ListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableList _observableList;

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

    public ListViewModel(ObservableList observableList)
    {
        _categoryService = IPlatformApplication.Current!.Services.GetService<ICategoryService>()!;
        _itemService = IPlatformApplication.Current.Services.GetService<IItemService>()!;
        _clipboardService = IPlatformApplication.Current.Services.GetService<IClipboardService>()!;
        ObservableList = observableList;
        Items = new ObservableCollection<ObservableItem>(observableList.Items);
        NewObservableItem = new ObservableItem(ObservableList.Id!);
        SortItems();
        LoadCategories().SafeFireAndForget();
    }

    private async Task LoadCategories()
    {
        var categories = await _categoryService.GetAllByListIdAsync(ObservableList.Id!);
        Categories = new ObservableCollection<ObservableCategory>(categories);
        CurrentCategory = Categories.FirstOrDefault(c =>
            c.Name == Shared.Constants.DefaultCategoryName
        );
        OnPropertyChanged(nameof(Categories));
        OnPropertyChanged(nameof(CurrentCategory));
        Logger.Log(
            $"Loaded {Categories.Count} categories for list {ObservableList.Id} from the database"
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
            CurrentCategory != null ? CurrentCategory.Name : Shared.Constants.DefaultCategoryName;

        // Add to list and database
        Logger.Log($"Adding item: {NewObservableItem.ToLoggableString()}");
        await _itemService.CreateOrUpdateAsync(NewObservableItem);
        var value = new ItemChangedDto(ObservableList.Id!, NewObservableItem);
        WeakReferenceMessenger.Default.Send(new ItemAddedToListMessage(value));
        Items.Add(NewObservableItem);
        Notifier.ShowToast($"Added: {NewObservableItem.Title}");

        // Make sure the UI is reset/updated
        NewObservableItem = new ObservableItem(ObservableList.Id!);
        SortItems();
        OnPropertyChanged(nameof(NewObservableItem));
    }

    [RelayCommand]
    private async Task RemoveItem(ObservableItem i)
    {
        await _itemService.DeleteAsync(i);
        ObservableList.Items.Remove(i);
        Items.Remove(i);
        var value = new ItemChangedDto(ObservableList.Id!, i);
        WeakReferenceMessenger.Default.Send(new ItemRemovedFromListMessage(value));
        OnPropertyChanged(nameof(ObservableList));
    }

    [RelayCommand]
    private async Task RemoveAllItems()
    {
        if (!await IsRequestConfirmedByUser())
            return;

        Items.Clear();
        await _itemService.DeleteAllByListIdAsync(ObservableList.Id!);
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
        await Shell.Current.Navigation.PushModalAsync(new DetailPage(i, ObservableList));
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        _clipboardService.CopyToClipboard(Items, Categories);
    }

    [RelayCommand]
    private void InsertFromClipboard()
    {
        _clipboardService.InsertFromClipboardAsync(Items, Categories, ObservableList.Id!);
    }

    public string ProcessedObservableItemListName
    {
        get
        {
            if (ObservableList.Name.Length > 15)
            {
                return ObservableList.Name[..15].Trim() + "...";
            }

            return ObservableList.Name;
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
