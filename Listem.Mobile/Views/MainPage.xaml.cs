using System.Diagnostics.CodeAnalysis;
using AsyncAwaitBestPractices;
using Listem.Mobile.Utilities;
using Listem.Mobile.ViewModel;
using Microsoft.Extensions.Logging;
#if __ANDROID__ || __IOS__
using CommunityToolkit.Maui.Core;
#endif

namespace Listem.Mobile.Views;

public partial class MainPage
{
  private readonly MainViewModel _viewModel;
  private const uint AnimationDuration = 400u;
  private bool _isMenuOpen;

  public MainPage(MainViewModel viewModel)
  {
    InitializeComponent();
    _viewModel = viewModel;
    _viewModel.InitialiseUser();
    BindingContext = _viewModel;

    StickyEntry.Submitted += (_, text) =>
    {
      _viewModel.AddListCommand.Execute(text);
    };
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    StickyEntry.SetVisibility(false);
    _viewModel.TriggerListPropertyChange();
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

  private void MenuButton_OnTap(object? sender, EventArgs e)
  {
    var cancellationTokenSource = new CancellationTokenSource();
    if (!_isMenuOpen)
    {
      OpenSettings(cancellationTokenSource).SafeFireAndForget();
      return;
    }
    CloseSettings(cancellationTokenSource).SafeFireAndForget();
  }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
  [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
  [SuppressMessage("ReSharper", "UnusedParameter.Local")]
  private async Task OpenSettings(CancellationTokenSource cancellationTokenSource)
  {
#if __ANDROID__ || __IOS__
    var statusBarColor = (Color)Application.Current!.Resources["MainPageBackStatusBarColour"];
    CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(statusBarColor);
    CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.LightContent);
    MainPageContent.CornerRadius = 20;
    // MainPageContent.HasShadow = true; // Doesn't work yet: https://github.com/dotnet/maui/issues/11025
    MenuButton.Source = "expand_neutral.png";
    var resize = MainPageContent.TranslateTo(Width * 0.6, 0, AnimationDuration);
    var scaleDown = MainPageContent.ScaleTo(0.85, AnimationDuration);
    var tasks = new List<Task> { resize, scaleDown };
    await Task.WhenAll(tasks).WaitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
#endif
    _isMenuOpen = true;
  }

  [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
  [SuppressMessage("ReSharper", "UnusedParameter.Local")]
  private async Task CloseSettings(CancellationTokenSource cancellationTokenSource)
  {
#if __ANDROID__ || __IOS__
    await MainPageContent.RotateYTo(0, AnimationDuration / 4);
    var scaleBack = MainPageContent.ScaleTo(1, AnimationDuration / 4);
    var resize = MainPageContent.TranslateTo(0, 0, AnimationDuration / 4);
    var tasks = new List<Task> { scaleBack, resize };
    await Task.WhenAll(tasks).WaitAsync(cancellationTokenSource.Token);
    MainPageContent.CornerRadius = 0;
    // MainPageContent.HasShadow = false; // Doesn't work yet: https://github.com/dotnet/maui/issues/11025
    MenuButton.Source = "menu_neutral.png";
    var statusBarColor = (Color)Application.Current!.Resources["StatusBarColor"];
    CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(statusBarColor);
    CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.DarkContent);
#endif
    _isMenuOpen = false;
  }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

  private void SignUpInOrOutButton_OnClicked(object? sender, EventArgs e)
  {
    CloseSettings(new CancellationTokenSource()).SafeFireAndForget();
    _viewModel.BackToStartPageCommand.Execute(null);
  }

  private void DeleteMyAccount_OnClicked(object? sender, EventArgs e)
  {
    CloseSettings(new CancellationTokenSource()).SafeFireAndForget();
    _viewModel.DeleteMyAccountCommand.Execute(null);
  }
}
