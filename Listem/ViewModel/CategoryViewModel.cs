using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Models;
using Listem.Services;
using Listem.Utilities;

namespace Listem.ViewModel;

public partial class CategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Category> _categories;

    [ObservableProperty]
    private Category _newCategory;
    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    public static string DefaultCategoryName => ICategoryService.DefaultCategoryName;

    public CategoryViewModel(ICategoryService categoryService, IItemService itemService)
    {
        Categories = [];
        NewCategory = new Category();
        _categoryService = categoryService;
        _itemService = itemService;
        LoadCategoriesFromDatabase()
            .SafeFireAndForget<Exception>(ex => Logger.Log($"Failed to load categories: {ex}"));
    }

    private async Task LoadCategoriesFromDatabase()
    {
        var loaded = await _categoryService.GetAllAsync().ConfigureAwait(false);
        Categories = new ObservableCollection<Category>(loaded);
        OnPropertyChanged(nameof(IsCollectionViewLargerThanThreshold));
        Logger.Log($"Loaded {loaded.Count} categories, new collection size: {Categories.Count}");
    }

    [RelayCommand]
    private async Task AddCategory(ITextInput view)
    {
        // Don't add empty items
        if (string.IsNullOrWhiteSpace(NewCategory.Name))
            return;

        // Pre-process
        NewCategory.Name = StringProcessor.TrimAndCapitaliseFirstChar(NewCategory.Name);

        // Only allow unique names
        if (Categories.Any(category => category.Name == NewCategory.Name))
        {
            Notifier.ShowToast($"Cannot add '{NewCategory.Name}' - it already exists");
            return;
        }

        // Add to list and database
        Categories.Add(NewCategory);
        await _categoryService.CreateOrUpdateAsync(NewCategory);

        // Make sure the UI is reset/updated
#if __ANDROID__
        var isKeyboardHidden = view.HideKeyboardAsync(CancellationToken.None);
        Logger.Log("Keyboard hidden: " + isKeyboardHidden);
#endif
        Notifier.ShowToast($"Added: {NewCategory.Name}");
        NewCategory = new Category();
        OnPropertyChanged(nameof(NewCategory));
        OnPropertyChanged(nameof(IsCollectionViewLargerThanThreshold));
    }

    [RelayCommand]
    private async Task RemoveCategory(Category category)
    {
        if (category.Name == ICategoryService.DefaultCategoryName)
        {
            Notifier.ShowToast("Cannot remove default category");
            return;
        }

        Categories.Remove(category);
        await _itemService.UpdateAllUsingCategoryAsync(category.Name);
        await _categoryService.DeleteAsync(category);
        OnPropertyChanged(nameof(IsCollectionViewLargerThanThreshold));
        Notifier.ShowToast($"Removed: {category.Name}");
    }

    [RelayCommand]
    private async Task ResetCategories()
    {
        if (!await IsRequestConfirmedByUser())
            return;

        Notifier.ShowToast("Reset categories");
        await _itemService.UpdateAllToDefaultCategoryAsync().ConfigureAwait(false);
        await _categoryService.DeleteAllAsync().ConfigureAwait(false);
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
