using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Views;
using Microsoft.Extensions.Logging;
#if __ANDROID__
using CommunityToolkit.Maui.Core.Platform;
#endif

namespace Listem.Mobile.ViewModel;

public partial class AuthViewModel : BaseViewModel
{
  private readonly IServiceProvider _serviceProvider;

  public AuthViewModel(IServiceProvider serviceProvider)
    : base(serviceProvider.GetService<ILogger<AuthViewModel>>()!)
  {
    _serviceProvider = serviceProvider;
    Logger.Info("On-device logs are stored at: {LogFilePath}", FileSystem.Current.AppDataDirectory);
  }

  [RelayCommand]
  private static async Task Back()
  {
    await Shell.Current.Navigation.PopModalAsync();
  }

  [RelayCommand]
  private static async Task OpenPrivacyPolicy()
  {
    await Launcher.OpenAsync(Constants.PrivacyPolicyUrl);
  }

  [RelayCommand]
  private async Task GoToMainPage()
  {
    await Shell.Current.Navigation.PushAsync(_serviceProvider.GetService<MainPage>());
  }

  // ReSharper disable once UnusedParameter.Local
  private void HideKeyboard(ITextInput view)
  {
#if __ANDROID__
    var isKeyboardHidden = view.HideKeyboardAsync(CancellationToken.None);
    Logger.Info("Keyboard hidden: {State}", isKeyboardHidden);
#endif
  }
}
