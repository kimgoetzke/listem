using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Models;
using Listem.Services;
using Listem.Utilities;

namespace Listem.ViewModel;

[QueryProperty(nameof(ObservableItem), nameof(ObservableItem))]
public partial class DetailViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ObservableCategory> _categories = [];

    [ObservableProperty] private ObservableCategory _currentCategory;

    [ObservableProperty] private ObservableItem _observableItem;

    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;

    public DetailViewModel(
        ObservableItem observableItem,
        ICategoryService categoryService,
        IItemService itemService
    )
    {
        ObservableItem = observableItem;
        CurrentCategory = new ObservableCategory(observableItem.ListId);
        _categoryService = categoryService;
        _itemService = itemService;
        SetCategories();
    }

    [RelayCommand]
    private async Task GoBack()
    {
        ObservableItem.CategoryName = CurrentCategory.Name;
        await _itemService.CreateOrUpdateAsync(ObservableItem);
        Notifier.ShowToast($"Updated: {ObservableItem.Title}");
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