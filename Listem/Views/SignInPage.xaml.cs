using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Core;
using Listem.ViewModel;

namespace Listem.Views;

public partial class SignInPage
{
    public SignInPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = serviceProvider.GetService<LoginViewModel>();
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
#if __ANDROID__ || __IOS__
        var statusBarColor = (Color)Application.Current!.Resources["AccentBright"];
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(statusBarColor);
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.LightContent);
#endif
    }
}
