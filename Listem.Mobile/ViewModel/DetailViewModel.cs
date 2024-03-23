using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using ListType = Listem.Shared.Enums.ListType;

namespace Listem.Mobile.ViewModel;

[QueryProperty(nameof(ObservableItem), nameof(ObservableItem))]
public partial class DetailViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ObservableCategory> _categories = [];

    [ObservableProperty]
    private ObservableCategory _currentCategory;

    [ObservableProperty]
    private ObservableItem _observableItem;

    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    public ListType ListType { get; }

    public DetailViewModel(
        ObservableItem observableItem,
        ObservableList observableList,
        ICategoryService categoryService,
        IItemService itemService
    )
    {
        ObservableItem = observableItem;
        ListType = observableList.ListType;
        CurrentCategory = new ObservableCategory(observableItem.ListId);
        _categoryService = categoryService;
        _itemService = itemService;
        SetCategories();
    }

    [RelayCommand]
    private async Task SaveAndBack()
    {
        ObservableItem.CategoryName = CurrentCategory.Name;
        await _itemService.CreateOrUpdateAsync(ObservableItem);
        Back().SafeFireAndForget();
    }

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

    private async void SetCategories()
    {
        var loaded = await _categoryService.GetAllByListIdAsync(ObservableItem.ListId);
        Categories.Clear();
        foreach (var category in loaded)
        {
            Categories.Add(category);
            if (category.Name == ObservableItem.CategoryName)
            {
                CurrentCategory = category;
            }
        }
    }
}
