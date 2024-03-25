using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using ListType = Listem.Mobile.Models.ListType;

namespace Listem.Mobile.ViewModel;

public partial class EditListViewModel : BaseViewModel
{
    public static string DefaultCategoryName => Constants.DefaultCategoryName;

    [ObservableProperty]
    private List _list;

    [ObservableProperty]
    private string _currentName;

    [ObservableProperty]
    private ListType _currentListType;

    [ObservableProperty]
    private ObservableCollection<ListType> _listTypes = [];

    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly IListService _listService;

    public EditListViewModel(List list, IServiceProvider serviceProvider)
    {
        _listService = serviceProvider.GetService<IListService>()!;
        _categoryService = serviceProvider.GetService<ICategoryService>()!;
        _itemService = serviceProvider.GetService<IItemService>()!;
        List = list;
        ListTypes = new ObservableCollection<ListType>(
            Enum.GetValues(typeof(ListType)).Cast<ListType>()
        );
        CurrentListType = Enum.TryParse(list.ListType, out ListType type)
            ? type
            : ListType.Standard;
        CurrentName = list.Name;
    }

    [RelayCommand]
    private async Task AddCategory(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var category = new Category { Name = StringProcessor.TrimAndCapitalise(text) };
        if (List.Categories.Any(c => c.Name == category.Name))
        {
            Notifier.ShowToast($"Cannot add '{category.Name}' - it already exists");
            return;
        }

        await _categoryService.CreateAsync(category, List);
        Notifier.ShowToast($"Added: {category.Name}");
    }

    [RelayCommand]
    private async Task RemoveCategory(Category category)
    {
        if (category.Name == Constants.DefaultCategoryName)
        {
            Notifier.ShowToast("Cannot remove default category");
            return;
        }

        var message = $"Removed: {category.Name}";
        await _itemService.ResetSelectedToDefaultCategoryAsync(List, category);
        await _categoryService.DeleteAsync(category);
        Notifier.ShowToast(message);
    }

    [RelayCommand]
    private async Task ResetCategories()
    {
        if (!await IsResetCategoriesRequestConfirmed())
            return;

        await _itemService.ResetAllToDefaultCategoryAsync(List);
        await _categoryService.ResetAsync(List);
        Notifier.ShowToast("Reset categories");
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

        await _itemService.DeleteAllInListAsync(List);
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
        var name = StringProcessor.TrimAndCapitalise(CurrentName);
        await _listService.UpdateAsync(List, name: name, listType: CurrentListType.ToString());
        await Back();
    }

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
