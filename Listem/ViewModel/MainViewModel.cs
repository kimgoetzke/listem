using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Models;
using Listem.Services;
using Listem.Utilities;
using Listem.Views;
using Random = System.Random;

namespace Listem.ViewModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ObservableItemList> _lists = [];

    [ObservableProperty]
    private ObservableItemList _newList;

    private readonly IItemListService _itemListService;
    private readonly IItemService _itemService;

    public MainViewModel(IItemListService itemListService, IItemService itemService)
    {
        _itemListService = itemListService;
        _itemService = itemService;
        NewList = new ObservableItemList();
        Lists = [];

        WeakReferenceMessenger.Default.Register<ItemRemovedFromListMessage>(
            this,
            (_, m) =>
            {
                Logger.Log(
                    $"Received message: Removing '{m.Value.Item.Title}' from {m.Value.ListId}"
                );
                Lists.First(l => l.Id == m.Value.ListId).Items.Remove(m.Value.Item);
            }
        );

        WeakReferenceMessenger.Default.Register<ItemAddedToListMessage>(
            this,
            (_, m) =>
            {
                Logger.Log($"Received message: Adding '{m.Value.Item.Title}' to {m.Value.ListId}");
                Lists.First(l => l.Id == m.Value.ListId).Items.Add(m.Value.Item);
            }
        );
    }

    public async Task LoadItemLists()
    {
        Logger.Log("Loading lists from database");
        var lists = await _itemListService.GetAllAsync();
        foreach (var list in lists)
        {
            var items = await _itemService.GetAllByListIdAsync(list.Id);
            foreach (var item in items)
            {
                list.Items.Add(item);
            }
            Lists.Add(list);
        }
        await AddTestData();
    }

    // TODO: Remove this method once testing is done
    private async Task AddTestData()
    {
        if (Lists.Count > 0)
            return;

        var list1 = new ObservableItemList { Name = "Shopping list" };
        list1.Items =
        [
            new ObservableItem(list1.Id) { Title = "Bread" },
            new ObservableItem(list1.Id) { Title = "Milk" },
            new ObservableItem(list1.Id) { Title = "Eggs" },
            new ObservableItem(list1.Id) { Title = "Butter" },
            new ObservableItem(list1.Id) { Title = "Cheese" },
            new ObservableItem(list1.Id) { Title = "Tomatoes" },
            new ObservableItem(list1.Id) { Title = "Potatoes" },
            new ObservableItem(list1.Id) { Title = "Carrots" },
            new ObservableItem(list1.Id) { Title = "Cucumber" },
            new ObservableItem(list1.Id) { Title = "Bananas" },
            new ObservableItem(list1.Id) { Title = "Apples" },
            new ObservableItem(list1.Id) { Title = "Oranges" },
            new ObservableItem(list1.Id) { Title = "Grapes" },
            new ObservableItem(list1.Id) { Title = "Pineapple" },
            new ObservableItem(list1.Id) { Title = "Peaches" }
        ];
        var list2 = new ObservableItemList { Name = "Todo list with a very, very long title" };
        list2.Items =
        [
            new ObservableItem(list2.Id) { Title = "Do this" },
            new ObservableItem(list2.Id) { Title = "Do that" },
            new ObservableItem(list2.Id)
            {
                Title = "Do something that is difficult to explain succinctly"
            }
        ];
        await AddNewList(list1);
        await AddItemsFromNewList(list1);
        await AddNewList(list2);
        await AddItemsFromNewList(list2);
        Logger.Log("Added lists for testing");
    }

    [RelayCommand]
    private async Task AddList()
    {
        // TODO: Remove this once testing is done
        var random = new Random();
        NewList = new ObservableItemList
        {
            Name = "List " + random.Next(100) + 1,
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };

        // Don't add list without name
        if (string.IsNullOrWhiteSpace(NewList.Name))
            return;

        // Process list name and add the list
        NewList.Name = StringProcessor.TrimAndCapitalise(NewList.Name);
        await AddNewList(NewList);

        // Add items from new list, if any
        if (NewList.Items.Count > 0)
        {
            await AddItemsFromNewList(NewList);
        }

        // Update UI
        NewList = new ObservableItemList();
        OnPropertyChanged(nameof(NewList));
        OnPropertyChanged(nameof(Lists));
    }

    private async Task AddNewList(ObservableItemList newList)
    {
        await _itemListService.CreateOrUpdateAsync(newList);
        Lists.Add(newList);
        Notifier.ShowToast($"Added: {newList.Name}");
        Logger.Log($"Added list: {newList.ToLoggableString()}");
    }

    private async Task AddItemsFromNewList(ObservableItemList list)
    {
        foreach (var item in list.Items)
        {
            await _itemService.CreateOrUpdateAsync(item);
        }
    }

    [RelayCommand]
    private async Task RemoveList(ObservableItemList list)
    {
        Logger.Log($"Removing list: {list.ToLoggableString()}");
        await Application.Current!.Dispatcher.DispatchAsync(() => Lists.Remove(list));
    }

    [RelayCommand]
    private static async Task TapList(ObservableItemList list)
    {
        Logger.Log($"Opening list: {list.ToLoggableString()}");
        await Shell.Current.Navigation.PushAsync(new ListPage(list));
    }
}
