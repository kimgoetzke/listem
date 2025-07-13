using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Listem.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace Listem.Mobile.ViewModel;

public partial class MainViewModel : BaseViewModel, IDisposable
{
  [ObservableProperty]
  private ObservableCollection<ObservableList> _lists = null!;

  [ObservableProperty]
  private ObservableList _newList;

  [ObservableProperty]
  private ObservableCollection<ObservableTheme> _themes = [];

  [ObservableProperty]
  private ObservableTheme _currentTheme;

  [ObservableProperty]
  private string _appVersion = "Version " + AppInfo.VersionString;

  private readonly IServiceProvider _serviceProvider;
  private readonly IListService _listService;
  private readonly IItemService _itemService;

  public MainViewModel(IServiceProvider serviceProvider)
    : base(serviceProvider.GetService<ILogger<MainViewModel>>()!)
  {
    IsBusy = true;
    _serviceProvider = serviceProvider;
    _listService = serviceProvider.GetService<IListService>()!;
    _itemService = serviceProvider.GetService<IItemService>()!;
    Logger.Debug("Initialising MainViewModel...");
    NewList = new ObservableList();
    Lists = [];
    Themes = ThemeHandler.GetAllThemesAsCollection();
    CurrentTheme = Themes.First(t => t.Name == Settings.CurrentTheme);

    WeakReferenceMessenger.Default.Register<ItemRemovedFromListMessage>(
      this,
      (_, m) =>
      {
        Logger.Info(
          "Received message: Removing {Title} from {ID}",
          m.Value.Item.Title,
          m.Value.ListId
        );
        try
        {
          Lists.First(l => l.Id == m.Value.ListId).Items.Remove(m.Value.Item);
        }
        catch (InvalidOperationException)
        {
          Logger.Error(
            "Failed to remove item {Title} from list {ID} because the list does not seem to exist",
            m.Value.Item.Title,
            m.Value.ListId
          );
        }
      }
    );

    WeakReferenceMessenger.Default.Register<ItemAddedToListMessage>(
      this,
      (_, m) =>
      {
        Logger.Info("Received message: Adding {Title} to {ID}", m.Value.Item.Title, m.Value.ListId);
        Lists.First(l => l.Id == m.Value.ListId).Items.Add(m.Value.Item);
      }
    );

    IsBusy = false;
  }

  public async Task LoadLists()
  {
    Logger.Info("Loading lists from database");
    var lists = await _listService.GetAllAsync();
    foreach (var list in lists)
    {
      var items = await _itemService.GetAllByListIdAsync(list.Id!);
      foreach (var item in items)
      {
        list.Items.Add(item);
      }

      Lists.Add(list);
    }

    SortLists();
  }

  private void SortLists()
  {
    Lists = new ObservableCollection<ObservableList>(Lists.OrderBy(l => l.UpdatedOn).Reverse());
    OnPropertyChanged(nameof(Lists));
  }

  [RelayCommand]
  private async Task AddList(string name)
  {
    if (name.Length == 0)
      return;

    var processedName = StringProcessor.TrimAndCapitalise(name);
    if (
      Lists.FirstOrDefault(l => l.Name.Equals(processedName, StringComparison.OrdinalIgnoreCase))
      != null
    )
    {
      await Notifier.ShowAlertAsync(
        "List already exists",
        $"A list with the name \"{processedName}\" already exists (ignoring case). Please choose a different name.",
        "OK"
      );
      return;
    }

    NewList = new ObservableList
    {
      Name = processedName,
      AddedOn = DateTime.Now,
      UpdatedOn = DateTime.Now
    };
    await AddNewList(NewList);

    NewList = new ObservableList();
    OnPropertyChanged(nameof(NewList));
    OnPropertyChanged(nameof(Lists));
  }

  private async Task AddNewList(ObservableList newList)
  {
    await _listService.CreateOrUpdateAsync(newList);
    Lists.Add(newList);
    Notifier.ShowToast($"Added: {newList.Name}");
    Logger.Info("Added list: {List}", newList.ToLoggableString());
    SortLists();
  }

  [RelayCommand]
  private async Task RemoveList(ObservableList list)
  {
    if (!await IsDeletionConfirmedByUser(list.Name))
    {
      Logger.Info("Cancelled action to delete: {List}", list.ToLoggableString());
      return;
    }

    await IsBusyWhile(async () =>
    {
      if (list.Id != null)
      {
        await _itemService.DeleteAllByListIdAsync(list.Id);
      }

      await _listService.DeleteAsync(list);
      Lists.Remove(list);
      OnPropertyChanged(nameof(Lists));
    });
  }

  private static async Task<bool> IsDeletionConfirmedByUser(string listName)
  {
    return await Notifier.ShowConfirmationAlertAsync(
      "Delete list",
      $"This will delete the list '{listName}' and all of its contents. It cannot be undone. Are you sure?"
    );
  }

  [RelayCommand]
  private async Task SetTheme(ObservableTheme? theme)
  {
    if (theme == null)
      return;

    Logger.Info("Changing theme to: {Theme}", theme);
    ThemeHandler.SetTheme(theme.Name);
    CurrentTheme = theme;
    OnPropertyChanged(nameof(CurrentTheme));
    await Shell.Current.Navigation.PopToRootAsync();
  }

  [RelayCommand]
  private async Task BackToStartPage()
  {
    await Shell.Current.Navigation.PopToRootAsync();
    Dispose();
  }

  [RelayCommand]
  private async Task TapList(ObservableList list)
  {
    Logger.Info("Opening list: {List}", list.ToLoggableString());
    var listPage = IsBusyWhile(() => new ListPage(list, _serviceProvider));
    await Shell.Current.Navigation.PushAsync(listPage);
  }

  [RelayCommand]
  private async Task EditList(ObservableList list)
  {
    Logger.Info("Editing list: {List}", list.ToLoggableString());
    var editListPage = IsBusyWhile(() => new EditListPage(list, _serviceProvider));
    await Shell.Current.Navigation.PushModalAsync(editListPage);
  }

  [RelayCommand]
  private async Task DeleteMyData()
  {
    if (
      !await Notifier.ShowConfirmationAlertAsync(
        "Delete all data",
        "This will delete all your user data and lists permanently. This action cannot be undone. Are you sure?"
      )
    )
      return;

    await IsBusyWhile(async () =>
    {
      foreach (var list in Lists)
      {
        if (list.Id != null)
          await _itemService.DeleteAllByListIdAsync(list.Id);
        await _listService.DeleteAsync(list);
      }

      OnPropertyChanged(nameof(Lists));
    });
    await Shell.Current.Navigation.PopToRootAsync();
  }

  public void Dispose()
  {
    Logger.Debug("Disposing MainViewModel...");
    GC.SuppressFinalize(this);
  }
}
