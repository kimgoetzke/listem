using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using ListType = Listem.Shared.Enums.ListType;

namespace Listem.Mobile.ViewModel;

[QueryProperty(nameof(Item), nameof(Item))]
public partial class DetailViewModel : ObservableObject
{
    [ObservableProperty]
    private IList<Category> _categories = [];

    [ObservableProperty]
    private Category _currentCategory;

    [ObservableProperty]
    private Item _item;

    private readonly IItemService _itemService;
    public ListType ListType { get; }

    public DetailViewModel(Item item, List list, IServiceProvider serviceProvider)
    {
        Item = item;
        ListType = Enum.TryParse(list.ListType, out ListType type) ? type : ListType.Standard;
        CurrentCategory = new Category();
        Categories = list.Categories;
        _itemService = serviceProvider.GetService<IItemService>()!;
    }

    [RelayCommand]
    private async Task SaveAndBack()
    {
        Item.Category = CurrentCategory;
        await _itemService.CreateOrUpdateAsync(Item);
        Back().SafeFireAndForget();
    }

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
