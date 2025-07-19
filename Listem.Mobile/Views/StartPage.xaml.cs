using Listem.Mobile.Utilities;
using Listem.Mobile.ViewModel;
using Microsoft.Extensions.Logging;

namespace Listem.Mobile.Views;

public partial class StartPage
{
  private readonly ILogger<StartPage> _logger;
  private readonly IServiceProvider _serviceProvider;

  public StartPage(IServiceProvider serviceProvider)
  {
    InitializeComponent();
    _serviceProvider = serviceProvider;
    _logger = _serviceProvider.GetService<ILogger<StartPage>>()!;
    var viewModel = _serviceProvider.GetService<AuthViewModel>()!;
    BindingContext = viewModel;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();

    if (!Settings.FirstRun)
      return;

    _logger.Info("First time running this application");
    Settings.FirstRun = false;
  }

  // Evaluates redirect when user comes back from sign in page - is evaluated before token is refreshed on
  // initial app startup, so we need ViewModel_PropertyChanged too; there's probably a cleaner way to do this
  protected override void OnNavigatedTo(NavigatedToEventArgs args)
  {
    base.OnNavigatedTo(args);
    SkipLogin();
  }

  private void SkipLogin()
  {
    _logger.Info("Skipping sign-in, redirecting to main page now...");
    Shell.Current.Navigation.PushAsync(_serviceProvider.GetService<MainPage>());
  }
}
