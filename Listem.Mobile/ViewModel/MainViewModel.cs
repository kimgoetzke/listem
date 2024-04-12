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
using Realms;

namespace Listem.Mobile.ViewModel;

public partial class MainViewModel : BaseViewModel
{
  [ObservableProperty]
  private IQueryable<List> _lists = null!;

  [ObservableProperty]
  private ObservableCollection<List> _observableLists = [];

  [ObservableProperty]
  private ObservableCollection<ObservableTheme> _themes = [];

  [ObservableProperty]
  private ObservableTheme _currentTheme;

  [ObservableProperty]
  private string? _currentUserEmail;

  [ObservableProperty]
  private bool _isUserSignedIn;

  private readonly IServiceProvider _serviceProvider;
  private readonly IListService _listService;
  private readonly IItemService _itemService;
  private readonly ILogger _logger;
  private readonly Realm _realm = RealmService.GetMainThreadRealm();

  public MainViewModel(IServiceProvider serviceProvider)
  {
    IsBusy = true;
    _serviceProvider = serviceProvider;
    _listService = serviceProvider.GetService<IListService>()!;
    _itemService = serviceProvider.GetService<IItemService>()!;
    _logger = serviceProvider.GetService<ILogger<MainViewModel>>()!;
    GetSortedLists();
    Themes = ThemeHandler.GetAllThemesAsCollection();
    CurrentTheme = Themes.First(t => t.Name == Settings.CurrentTheme);

    WeakReferenceMessenger.Default.Register<UserStatusChangedMessage>(
      this,
      (_, m) =>
      {
        _logger.Info(
          "[MainViewModel] Received message: Current user status has changed to: {User}",
          m.Value
        );
        CurrentUserEmail = m.Value.EmailAddress;
        IsUserSignedIn = m.Value.IsSignedIn;
      }
    );
    IsBusy = false;
  }

  // TODO: Implement sorting and filtering
  private void GetSortedLists()
  {
    Lists = _realm.All<List>().OrderByDescending(l => l.UpdatedOn);
    OnPropertyChanged(nameof(Lists));
    UpdateObservableLists();
  }

  // Pretty nasty but the UI doesn't update when changes are made to the Realm collection, so this is
  // an attempt to make sure the list summaries are always reflecting the current state of the Realm.
  // Could possibly remove the Lists property entirely but need to test if that would work.
  private void UpdateObservableLists()
  {
    ObservableLists.Clear();
    foreach (var list in Lists)
    {
      ObservableLists.Add(list);
    }
  }

  public void InitialiseUser()
  {
    var currentUser = RealmService.User;
    CurrentUserEmail = currentUser.EmailAddress;
    IsUserSignedIn = currentUser.IsSignedIn;
  }

  public void TriggerListPropertyChange()
  {
    OnPropertyChanged(nameof(Lists));
    UpdateObservableLists();
  }

  [RelayCommand]
  private async Task AddList(string name)
  {
    if (name.Length == 0)
      return;

    var newList = new List
    {
      Name = StringProcessor.TrimAndCapitalise(name),
      OwnedBy = RealmService.User.Id!,
      ListType = ListType.Standard.ToString(),
      UpdatedOn = DateTime.Now
    };
    await _listService.CreateAsync(newList);
    GetSortedLists();
  }

  [RelayCommand]
  private async Task RemoveList(List list)
  {
    if (!await IsDeletionConfirmedByUser(list.Name))
    {
      _logger.Info("Cancelled action to delete: {List}", list.ToLog());
      return;
    }

    IsBusy = true;
    await _itemService.DeleteAllInListAsync(list);
    await _listService.DeleteAsync(list);
    OnPropertyChanged(nameof(Lists));
    UpdateObservableLists();
    IsBusy = false;
  }

  private static async Task<bool> IsDeletionConfirmedByUser(string listName)
  {
    return await Notifier.ShowConfirmationAlertAsync(
      "Delete list",
      $"This will delete the list '{listName}' and all of its contents. It cannot be undone. Are you sure?"
    );
  }

  [RelayCommand]
  private async Task ExitList(List list)
  {
    if (
      !await Notifier.ShowConfirmationAlertAsync(
        "Exit list",
        "You don't own this list but you can remove yourself from it without deleting it for the owner. Do you want to continue?"
      )
    )
    {
      _logger.Info("Cancelled action to exit list: {List}", list.ToLog());
      return;
    }

    IsBusy = true;
    var id = list.SharedWith.First(id => id == RealmService.User.Id);
    if (await _listService.RevokeAccess(list, id))
    {
      ObservableLists.Remove(list);
    }
    else
    {
      await Notifier.ShowAlertAsync(
        "Failed leave shared list",
        "An error occurred while trying to remove yourself from this shared list. Please try again and contact the developer if this issue persists.",
        "OK"
      );
    }
    IsBusy = false;
  }

  [RelayCommand]
  private async Task SetTheme(ObservableTheme? theme)
  {
    if (theme == null)
      return;

    _logger.Info("Changing theme to: {Theme}", theme);
    ThemeHandler.SetTheme(theme.Name);
    CurrentTheme = theme;
    OnPropertyChanged(nameof(CurrentTheme));
    await Shell.Current.Navigation.PopToRootAsync();
  }

  [RelayCommand]
  private async Task BackToStartPage()
  {
    IsBusy = true;
    await RealmService.SignOutAsync();
    IsUserSignedIn = false;
    IsBusy = false;
    await Shell.Current.Navigation.PopToRootAsync();
  }

  [RelayCommand]
  private async Task TapList(List list)
  {
    _logger.Info("Opening list: {List}", list.ToLog());
    await Shell.Current.Navigation.PushAsync(new ListPage(list, _serviceProvider));
  }

  [RelayCommand]
  private async Task EditList(List list)
  {
    _logger.Info("Editing list: {List}", list.ToLog());
    await Shell.Current.Navigation.PushModalAsync(new EditListPage(list, _serviceProvider));
  }
}
