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
  }

  public void InitialiseUser()
  {
    var currentUser = RealmService.User;
    CurrentUserEmail = currentUser.EmailAddress;
    IsUserSignedIn = currentUser.IsSignedIn;
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
      UpdatedOn = DateTime.Now.ToUniversalTime()
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
    IsBusy = false;
  }

  private static async Task<bool> IsDeletionConfirmedByUser(string listName)
  {
    return await Shell.Current.DisplayAlert(
      "Delete list",
      $"This will delete the list '{listName}' and all of its contents. It cannot be undone. Are you sure?",
      "Yes",
      "No"
    );
  }

  [RelayCommand]
  private async Task ExitList(List list)
  {
    if (
      !await Notifier.ShowConfirmationAlertAsync(
        "Exit list",
        "You don't own this list but you can remove yourself from this list without deleting it for the owner. Do you want to continue?"
      )
    )
    {
      _logger.Info("Cancelled action to exit list: {List}", list.ToLog());
      return;
    }

    IsBusy = true;
    CurrentUserEmail = CurrentUserEmail!.ToLower();
    await _listService.RevokeAccess(list, CurrentUserEmail);
    IsBusy = false;
  }

  [RelayCommand]
  private Task SetTheme(ObservableTheme? theme)
  {
    if (theme == null)
      return Task.CompletedTask;

    _logger.Info("Changing theme to: {Theme}", theme);
    ThemeHandler.SetTheme(theme.Name);
    CurrentTheme = theme;
    OnPropertyChanged(nameof(CurrentTheme));
    return Task.CompletedTask;
  }

  [RelayCommand]
  private async Task BackToStartPage()
  {
    IsBusy = true;
    await RealmService.SignOutAsync();
    await Shell.Current.Navigation.PopToRootAsync();
    IsUserSignedIn = false;
    IsBusy = false;
  }

  [RelayCommand]
  private async Task TapList(List list)
  {
    IsBusy = true;
    _logger.Info("Opening list: {List}", list.ToLog());
    await Shell.Current.Navigation.PushAsync(new ListPage(list, _serviceProvider));
    IsBusy = false;
  }

  [RelayCommand]
  private async Task EditList(List list)
  {
    IsBusy = true;
    _logger.Info("Editing list: {List}", list.ToLog());
    await Shell.Current.Navigation.PushModalAsync(new EditListPage(list, _serviceProvider));
    IsBusy = false;
  }
}
