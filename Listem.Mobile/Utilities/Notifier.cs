using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using Listem.Mobile.Views;

namespace Listem.Mobile.Utilities;

public static class Notifier
{
  public static void ShowToast(string message)
  {
    var cancellationTokenSource = new CancellationTokenSource();
    Toast.Make(message).Show(cancellationTokenSource.Token);
  }

  public static Func<Task> ShowActivityIndicator()
  {
    var popup = new BusyPopup();
    Shell.Current.CurrentPage.ShowPopup(popup);
    return () => Shell.Current.CurrentPage.ClosePopupAsync();
  }

  public static Task ShowAlertAsync(string title, string message, string button)
  {
    return Shell.Current.DisplayAlertAsync(title, message, button);
  }

  public static async Task<bool> ShowConfirmationAlertAsync(string title, string message)
  {
    return await Shell.Current.DisplayAlertAsync(title, message, "Yes", "No");
  }
}
