using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using Listem.Mobile.Views;

namespace Listem.Mobile.Utilities;

public static class Notifier
{
    public static void ShowToast(string message)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        Toast.Make(message).Show(cancellationTokenSource.Token);
    }

    public static Action ShowActivityIndicator()
    {
        var popup = new BusyPopup();
        Shell.Current.ShowPopup(popup);
        return () => popup.Close();
    }

    public static Task ShowAlertAsync(string title, string message, string button)
    {
        return Shell.Current.DisplayAlert(title, message, button);
    }
}
