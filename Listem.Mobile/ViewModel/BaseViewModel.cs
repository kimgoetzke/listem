using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Mobile.Utilities;
using Microsoft.Extensions.Logging;

namespace Listem.Mobile.ViewModel;

public partial class BaseViewModel(ILogger<BaseViewModel> logger) : ObservableObject
{
  protected readonly ILogger<BaseViewModel> Logger = logger;

  [ObservableProperty]
  private bool _isBusy;

  private Action? _currentDismissAction;

  partial void OnIsBusyChanged(bool value)
  {
    if (value)
    {
      _currentDismissAction = Notifier.ShowActivityIndicator();
    }
    else
    {
      _currentDismissAction?.Invoke();
      _currentDismissAction = null;
    }
  }

  protected async Task IsBusyWhile(Func<Task> request)
  {
    IsBusy = true;
    try
    {
      await request.Invoke();
    }
    finally
    {
      IsBusy = false;
    }
  }

  protected T IsBusyWhile<T>(Func<T> request)
  {
    IsBusy = true;
    try
    {
      return request.Invoke();
    }
    finally
    {
      IsBusy = false;
    }
  }
}
