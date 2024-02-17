using CommunityToolkit.Maui.Alerts;

namespace Listem.Utilities;

public static class Notifier
{
    public static void ShowToast(string message)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        Toast.Make(message).Show(cancellationTokenSource.Token);
    }
}
