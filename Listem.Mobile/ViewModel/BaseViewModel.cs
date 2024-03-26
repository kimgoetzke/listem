using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Mobile.Utilities;

namespace Listem.Mobile.ViewModel;

public partial class BaseViewModel : ObservableObject
{
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
}
