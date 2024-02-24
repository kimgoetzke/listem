using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Models;
using Listem.Services;
using Listem.Utilities;
using Listem.Views;

namespace Listem.ViewModel;

public partial class EditListViewModel : ObservableObject
{
    public static string DefaultCategoryName => ICategoryService.DefaultCategoryName;

    [ObservableProperty]
    private ObservableItemList _observableItemList;

    [ObservableProperty]
    private ObservableCollection<ObservableItem> _items = [];

    [ObservableProperty]
    private ObservableCategory _newObservableCategory;

    [ObservableProperty]
    private ObservableCollection<ObservableCategory> _categories = [];

    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly IItemListService _itemListService;

    public EditListViewModel(ObservableItemList observableItemList)
    {
        _itemListService = IPlatformApplication.Current?.Services.GetService<IItemListService>()!;
        _categoryService = IPlatformApplication.Current?.Services.GetService<ICategoryService>()!;
        _itemService = IPlatformApplication.Current?.Services.GetService<IItemService>()!;
        ObservableItemList = observableItemList;
        Items = new ObservableCollection<ObservableItem>(observableItemList.Items);
        NewObservableCategory = new ObservableCategory(observableItemList.Id);
        LoadCategories().SafeFireAndForget();
    }

    private async Task LoadCategories()
    {
        var categories = await _categoryService.GetAllByListIdAsync(ObservableItemList.Id);
        Categories = new ObservableCollection<ObservableCategory>(categories);
        OnPropertyChanged(nameof(Categories));
        Logger.Log(
            $"Loaded {Categories.Count} categories for list {ObservableItemList.Id} from database"
        );
    }

    [RelayCommand]
    private async Task AddCategory(string text)
    {
        // Don't add empty category
        if (string.IsNullOrWhiteSpace(text))
            return;

        // Pre-process
        NewObservableCategory.Name = StringProcessor.TrimAndCapitalise(text);

        // Only allow unique names
        if (Categories.Any(category => category.Name == NewObservableCategory.Name))
        {
            Notifier.ShowToast($"Cannot add '{NewObservableCategory.Name}' - it already exists");
            return;
        }

        // Add to list and database
        Categories.Add(NewObservableCategory);
        await _categoryService.CreateOrUpdateAsync(NewObservableCategory);

        // Update/reset UI
        Notifier.ShowToast($"Added: {NewObservableCategory.Name}");
        NewObservableCategory = new ObservableCategory(ObservableItemList.Id);
        OnPropertyChanged(nameof(NewObservableCategory));
    }

    [RelayCommand]
    private async Task RemoveCategory(ObservableCategory observableCategory)
    {
        if (observableCategory.Name == ICategoryService.DefaultCategoryName)
        {
            Notifier.ShowToast("Cannot remove default category");
            return;
        }

        Categories.Remove(observableCategory);
        await _itemService.UpdateAllToCategoryAsync(observableCategory.Name, ObservableItemList.Id);
        await _categoryService.DeleteAsync(observableCategory);
        Notifier.ShowToast($"Removed: {observableCategory.Name}");
    }

    [RelayCommand]
    private async Task ResetCategories()
    {
        if (!await IsResetCategoriesRequestConfirmed())
            return;

        Notifier.ShowToast("Reset categories");
        await _itemService
            .UpdateAllToDefaultCategoryAsync(ObservableItemList.Id)
            .ConfigureAwait(false);
        await _categoryService.DeleteAllByListIdAsync(ObservableItemList.Id).ConfigureAwait(false);
        await LoadCategories().ConfigureAwait(false);
    }

    private static Task<bool> IsResetCategoriesRequestConfirmed()
    {
        return Shell.Current.DisplayAlert(
            "Reset categories",
            $"This will remove all categories, except the '(Default)' one. Are you sure you want to continue?",
            "Yes",
            "No"
        );
    }

    [RelayCommand]
    private async Task RemoveAllItems()
    {
        if (!await IsRemoveItemsRequestConfirmed())
            return;

        Items.Clear();
        await _itemService.DeleteAllByListIdAsync(ObservableItemList.Id);
        Notifier.ShowToast("Removed all items from list");
    }

    private static async Task<bool> IsRemoveItemsRequestConfirmed()
    {
        return await Shell.Current.DisplayAlert(
            "Clear list",
            $"This will remove all items from your list. Are you sure you want to continue?",
            "Yes",
            "No"
        );
    }

    [RelayCommand]
    private async Task SaveAndBack()
    {
        ObservableItemList.Name = StringProcessor.TrimAndCapitalise(ObservableItemList.Name);
        await _itemListService.CreateOrUpdateAsync(ObservableItemList);
        Back().SafeFireAndForget();
    }

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
