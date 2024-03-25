using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using ListType = Listem.Shared.Enums.ListType;

namespace Listem.Mobile.ViewModel;

public partial class EditListViewModel : ObservableObject
{
    public static string DefaultCategoryName => Constants.DefaultCategoryName;

    [ObservableProperty]
    private List _list;

    [ObservableProperty]
    private ObservableCollection<ListType> _listTypes = [];

    [ObservableProperty]
    private Category _newCategory;

    [ObservableProperty]
    private IQueryable<Category> _categories;

    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly IListService _listService;

    public EditListViewModel(List list, IServiceProvider serviceProvider)
    {
        _listService = serviceProvider.GetService<IListService>()!;
        _categoryService = serviceProvider.GetService<ICategoryService>()!;
        _itemService = serviceProvider.GetService<IItemService>()!;
        List = list;
        // TODO: Current list type isn't selected
        ListTypes = new ObservableCollection<ListType>(
            Enum.GetValues(typeof(ListType)).Cast<ListType>()
        );
        // TODO: Fix below to make sure that categories are updated when item is added/removed
        Categories = new EnumerableQuery<Category>(list.Categories);
        NewCategory = new Category();
    }

    [RelayCommand]
    private async Task AddCategory(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        NewCategory.Name = StringProcessor.TrimAndCapitalise(text);
        if (Categories.Any(category => category.Name == NewCategory.Name))
        {
            Notifier.ShowToast($"Cannot add '{NewCategory.Name}' - it already exists");
            return;
        }

        await _categoryService.CreateAsync(NewCategory, List);
        Notifier.ShowToast($"Added: {NewCategory.Name}");
        NewCategory = new Category();
        OnPropertyChanged(nameof(NewCategory));
    }

    [RelayCommand]
    private async Task RemoveCategory(Category category)
    {
        if (category.Name == Shared.Constants.DefaultCategoryName)
        {
            Notifier.ShowToast("Cannot remove default category");
            return;
        }

        var message = $"Removed: {category.Name}";
        await _itemService.UpdateAllToCategoryAsync(category.Name, List.Id);
        await _categoryService.DeleteAsync(category);
        Notifier.ShowToast(message);
    }

    [RelayCommand]
    private async Task ResetCategories()
    {
        if (!await IsResetCategoriesRequestConfirmed())
            return;

        Notifier.ShowToast("Reset categories");
        await _itemService.UpdateAllToDefaultCategoryAsync(List.Id).ConfigureAwait(false);
        await _categoryService.ResetAsync(List).ConfigureAwait(false);
    }

    private static Task<bool> IsResetCategoriesRequestConfirmed()
    {
        return Shell.Current.DisplayAlert(
            "Reset categories",
            $"This will remove all categories. Are you sure you want to continue?",
            "Yes",
            "No"
        );
    }

    [RelayCommand]
    private async Task RemoveAllItems()
    {
        if (!await IsRemoveItemsRequestConfirmed())
            return;

        await _itemService.DeleteAllByListIdAsync(List.Id);
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
        List.Name = StringProcessor.TrimAndCapitalise(List.Name);
        await _listService.CreateOrUpdateAsync(List);
        await Back();
    }

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
