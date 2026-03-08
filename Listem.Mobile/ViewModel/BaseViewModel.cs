using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Mobile.Utilities;
using Microsoft.Extensions.Logging;

namespace Listem.Mobile.ViewModel;

public partial class BaseViewModel(ILogger<BaseViewModel> logger) : ObservableObject
{
  protected readonly ILogger<BaseViewModel> Logger = logger;

  [ObservableProperty]
  private bool _isBusy;

  private Func<Task>? _currentDismissAction;
  private bool _isAlreadyBusy;

  partial void OnIsBusyChanged(bool value)
  {
    if (_isAlreadyBusy)
      return;

    if (value)
      _currentDismissAction = Notifier.ShowActivityIndicator();
    else
    {
      _currentDismissAction?.Invoke().SafeFireAndForget();
      _currentDismissAction = null;
    }
  }

  protected async Task IsBusyWhile(Func<Task> request)
  {
    if (_isAlreadyBusy)
    {
      await request.Invoke();
      return;
    }

    _isAlreadyBusy = true;
    IsBusy = true;
    var dismiss = Notifier.ShowActivityIndicator();
    try
    {
      await request.Invoke();
    }
    finally
    {
      await dismiss();
      IsBusy = false;
      _isAlreadyBusy = false;
    }
  }
}
