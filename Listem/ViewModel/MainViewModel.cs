using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Models;
using Listem.Utilities;
using Listem.Views;
using Random = System.Random;

namespace Listem.ViewModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ItemList> _lists = [];

    [ObservableProperty]
    private ItemList _newList;

    public MainViewModel()
    {
        NewList = new ItemList();
        var list1 = new ItemList { Name = "Shopping list" };
        list1.Items =
        [
            new Item { Title = "Bread", ListId = list1.Id },
            new Item { Title = "Milk", ListId = list1.Id },
            new Item { Title = "Eggs", ListId = list1.Id },
            new Item { Title = "Butter", ListId = list1.Id },
            new Item { Title = "Cheese", ListId = list1.Id },
            new Item { Title = "Tomatoes", ListId = list1.Id },
            new Item { Title = "Potatoes", ListId = list1.Id },
            new Item { Title = "Carrots", ListId = list1.Id },
            new Item { Title = "Cucumber", ListId = list1.Id },
            new Item { Title = "Bananas", ListId = list1.Id },
            new Item { Title = "Apples", ListId = list1.Id },
            new Item { Title = "Oranges", ListId = list1.Id },
            new Item { Title = "Grapes", ListId = list1.Id },
            new Item { Title = "Pineapple", ListId = list1.Id },
            new Item { Title = "Peaches", ListId = list1.Id },
        ];
        var list2 = new ItemList { Name = "Todo list with a very, very long title" };
        list2.Items =
        [
            new Item { Title = "Do this", ListId = list2.Id },
            new Item { Title = "Do that", ListId = list2.Id },
            new Item
            {
                Title = "Do something that is very difficult to explain succinctly",
                ListId = list1.Id
            }
        ];
        Lists = [list1, list2];
    }

    [RelayCommand]
    private async Task AddList()
    {
        var random = new Random();
        NewList = new ItemList
        {
            Name = "List " + random.Next(100) + 1,
            Items = [],
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };
        if (string.IsNullOrWhiteSpace(NewList.Name))
            return;

        NewList.Name = StringProcessor.TrimAndCapitaliseFirstChar(NewList.Name);
        await Application.Current!.Dispatcher.DispatchAsync(() => Lists.Add(NewList));
        Notifier.ShowToast($"Added: {NewList.Name}");
        Logger.Log($"Added item: {NewList.ToLoggableString()}");

        // Make sure the UI is reset/updated
        NewList = new ItemList();
        OnPropertyChanged(nameof(NewList));
        OnPropertyChanged(nameof(Lists));
    }

    [RelayCommand]
    private async Task RemoveList(ItemList list)
    {
        Logger.Log($"Removing list: {list.ToLoggableString()}");
        await Application.Current!.Dispatcher.DispatchAsync(() => Lists.Remove(list));
    }

    [RelayCommand]
    private static async Task TapList(ItemList list)
    {
        Logger.Log($"Opening list: {list.ToLoggableString()}");
        await Shell.Current.Navigation.PushAsync(new ListPage(list));
    }
}
