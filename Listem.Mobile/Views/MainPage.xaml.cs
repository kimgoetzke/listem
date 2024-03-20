using System.Diagnostics.CodeAnalysis;
using AsyncAwaitBestPractices;
using Listem.Mobile.Utilities;
using Listem.Mobile.ViewModel;
#if __ANDROID__ || __IOS__
using CommunityToolkit.Maui.Core;
#endif

// ReSharper disable UnusedMember.Local

namespace Listem.Mobile.Views;

public partial class MainPage
{
    private readonly MainViewModel _viewModel;
    private const uint AnimationDuration = 400u;
    private bool _isMenuOpen;
    private Frame _frame = new();

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.InitialiseUser().SafeFireAndForget();
        _viewModel.LoadLists().SafeFireAndForget();

        StickyEntry.Submitted += (_, text) =>
        {
            _viewModel.AddListCommand.Execute(text);
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StickyEntry.SetVisibility(false);

        if (!Settings.FirstRun)
            return;

        Logger.Log("First time running this application");
        Settings.FirstRun = false;
    }

    private void AddListButton_OnClicked(object? sender, EventArgs e)
    {
        if (!_viewModel.AddListCommand.CanExecute(null))
            return;

        StickyEntry.SetVisibility(true);
    }

    private void MenuGrid_OnTapGridArea(object? sender, TappedEventArgs e)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        CloseSettings(cancellationTokenSource).SafeFireAndForget();
    }

    private async void MenuButton_OnTap(object sender, EventArgs e)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        if (!_isMenuOpen)
        {
            await OpenSettings(cancellationTokenSource);
            _isMenuOpen = true;
            return;
        }

        await CloseSettings(cancellationTokenSource);
        _isMenuOpen = false;
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    // ReSharper disable once UnusedParameter.Local
    private async Task OpenSettings(CancellationTokenSource cancellationTokenSource)
    {
#if __ANDROID__ || __IOS__
        var statusBarColor = (Color)Application.Current!.Resources["AccentBright"];
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(statusBarColor);
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.LightContent);
        MainPageContent.CornerRadius = 20;
        MenuButton.Source = "expand_neutral.png";
        var resize = MainPageContent.TranslateTo(Width * 0.6, 0, AnimationDuration);
        var scaleDown = MainPageContent.ScaleTo(0.85, AnimationDuration);
        var tasks = new List<Task> { resize, scaleDown };
        await Task.WhenAll(tasks).WaitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
#endif
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    // ReSharper disable once UnusedParameter.Local
    private async Task CloseSettings(CancellationTokenSource cancellationTokenSource)
    {
#if __ANDROID__ || __IOS__
        await MainPageContent.RotateYTo(0, AnimationDuration / 4);
        var scaleBack = MainPageContent.ScaleTo(1, AnimationDuration / 4);
        var resize = MainPageContent.TranslateTo(0, 0, AnimationDuration / 4);
        var tasks = new List<Task> { scaleBack, resize };
        await Task.WhenAll(tasks).WaitAsync(cancellationTokenSource.Token);
        MainPageContent.CornerRadius = 0;
        MenuButton.Source = "menu_neutral.png";
        var statusBarColor = (Color)Application.Current!.Resources["StatusBarColor"];
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(statusBarColor);
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.DarkContent);
#endif
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    private void SignUpOrSignInButton_OnClicked(object? sender, EventArgs e)
    {
        CloseSettings(new CancellationTokenSource()).SafeFireAndForget();
        _viewModel.BackToStartPageCommand.Execute(null);
    }

    private void SignOutButton_OnClicked(object? sender, EventArgs e)
    {
        CloseSettings(new CancellationTokenSource()).SafeFireAndForget();
        _viewModel.BackToStartPageCommand.Execute(null);
    }
}
