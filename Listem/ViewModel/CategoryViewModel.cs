using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Models;
using Listem.Services;
using Listem.Utilities;
using static Listem.Services.IService;

namespace Listem.ViewModel;

public partial class CategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ObservableCategory> _categories;

    [ObservableProperty]
    private ObservableCategory _newObservableCategory;
    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly string _listId;
    public static string DefaultCategoryName => ICategoryService.DefaultCategoryName;

    public CategoryViewModel(IReadOnlyCollection<IService> services, string listId)
    {
        _categoryService = (
            services.First(s => s.Type == ServiceType.Category) as ICategoryService
        )!;
        _itemService = (services.First(s => s.Type == ServiceType.Item) as IItemService)!;
        _listId = listId;
        Categories = [];
        NewObservableCategory = new ObservableCategory(listId);
        LoadCategoriesFromDatabase()
            .SafeFireAndForget<Exception>(ex => Logger.Log($"Failed to load categories: {ex}"));
    }

    private async Task LoadCategoriesFromDatabase()
    {
        var loaded = await _categoryService.GetAllByListIdAsync(_listId).ConfigureAwait(false);
        Categories = new ObservableCollection<ObservableCategory>(loaded);
        OnPropertyChanged(nameof(IsCollectionViewLargerThanThreshold));
        Logger.Log($"Loaded {loaded.Count} categories, new collection size: {Categories.Count}");
    }

    [RelayCommand]
    private async Task AddCategory(ITextInput view)
    {
        // Don't add empty items
        if (string.IsNullOrWhiteSpace(NewObservableCategory.Name))
            return;

        // Pre-process
        NewObservableCategory.Name = StringProcessor.TrimAndCapitalise(
            NewObservableCategory.Name
        );

        // Only allow unique names
        if (Categories.Any(category => category.Name == NewObservableCategory.Name))
        {
            Notifier.ShowToast($"Cannot add '{NewObservableCategory.Name}' - it already exists");
            return;
        }

        // Add to list and database
        Categories.Add(NewObservableCategory);
        await _categoryService.CreateOrUpdateAsync(NewObservableCategory);

        // Make sure the UI is reset/updated
#if __ANDROID__
        var isKeyboardHidden = view.HideKeyboardAsync(CancellationToken.None);
        Logger.Log("Keyboard hidden: " + isKeyboardHidden);
#endif
        Notifier.ShowToast($"Added: {NewObservableCategory.Name}");
        NewObservableCategory = new ObservableCategory(_listId);
        OnPropertyChanged(nameof(NewObservableCategory));
        OnPropertyChanged(nameof(IsCollectionViewLargerThanThreshold));
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
        await _itemService.UpdateAllToCategoryAsync(observableCategory.Name, _listId);
        await _categoryService.DeleteAsync(observableCategory);
        OnPropertyChanged(nameof(IsCollectionViewLargerThanThreshold));
        Notifier.ShowToast($"Removed: {observableCategory.Name}");
    }

    [RelayCommand]
    private async Task ResetCategories()
    {
        if (!await IsRequestConfirmedByUser())
            return;

        Notifier.ShowToast("Reset categories");
        await _itemService.UpdateAllToDefaultCategoryAsync(_listId).ConfigureAwait(false);
        await _categoryService.DeleteAllByListIdAsync(_listId).ConfigureAwait(false);
        await LoadCategoriesFromDatabase().ConfigureAwait(false);
    }

    private static Task<bool> IsRequestConfirmedByUser()
    {
        return Shell.Current.DisplayAlert(
            "Reset categories",
            $"This will remove all categories, except 'Any'. Are you sure you want to continue?",
            "Yes",
            "No"
        );
    }

    [RelayCommand]
    private static async Task GoBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    // Used to toggle on/off the line separator between categories list and buttons
    public bool IsCollectionViewLargerThanThreshold
    {
        get
        {
            const int itemHeight = 45; // As defined in Styles.XAML
            var currentHeight = Categories.Count * itemHeight;
            return currentHeight >= 270;
        }
    }
}
