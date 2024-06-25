using System.Text.RegularExpressions;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Listem.Mobile.Views;
using Microsoft.Extensions.Logging;
using User = Listem.Mobile.Models.User;
#if __ANDROID__
using CommunityToolkit.Maui.Core.Platform;
#endif

namespace Listem.Mobile.ViewModel;

public partial class AuthViewModel : BaseViewModel
{
  [ObservableProperty]
  private bool _isUserRegistered;

  [ObservableProperty]
  private bool _isUserSignedIn;

  [ObservableProperty]
  private Status _userStatus;

  [ObservableProperty]
  private string? _userEmail;

  [ObservableProperty]
  private string? _password;

  [ObservableProperty]
  private string? _passwordConfirmed;

  private readonly IServiceProvider _serviceProvider;

  public AuthViewModel(IServiceProvider serviceProvider)
    : base(serviceProvider.GetService<ILogger<AuthViewModel>>()!)
  {
    _serviceProvider = serviceProvider;
    Initialise();
  }

  private async void Initialise()
  {
    IsBusy = true;
    WeakReferenceMessenger.Default.Register<UserStatusChangedMessage>(
      this,
      (_, m) =>
      {
        Logger.Info(
          "[AuthViewModel] Received message: Current user status has changed to: {User}",
          m.Value
        );
        UpdateUser(m.Value);
      }
    );
    Logger.Info("On-device logs are stored at: {LogFilePath}", FileSystem.Current.AppDataDirectory);

    if (!await RealmService.Init())
      Notifier.ShowToast(Constants.TokenRefreshFailedMessage);

    IsBusy = false;
  }

  private void UpdateUser(User user)
  {
    IsUserRegistered = user.IsRegistered;
    IsUserSignedIn = user.IsSignedIn;
    UserEmail = user.EmailAddress;
    UserStatus = user.Status;
  }

  public void RedirectIfUserIsSignedIn()
  {
    if (!IsUserSignedIn)
      return;

    IsBusy = true;
    Logger.Info("User is signed in, redirecting now...");
    Shell.Current.Navigation.PushAsync(_serviceProvider.GetService<MainPage>());
    IsBusy = false;
  }

  [RelayCommand]
  private static async Task Back()
  {
    await Shell.Current.Navigation.PopModalAsync();
  }

  [RelayCommand]
  private async Task SignUp(ITextInput view)
  {
    if (!IsInputValid())
      return;

    if (Password != PasswordConfirmed)
    {
      await Notifier.ShowAlertAsync("Sign up failed", "Passwords do not match", "OK");
      return;
    }

    try
    {
      IsBusy = true;
      UserEmail = UserEmail!.ToLower();
      await RealmService.SignUpAsync(UserEmail, Password!);
      HideKeyboard(view);
      await Shell.Current.Navigation.PopAsync();
      IsUserRegistered = true;
      Password = null;
      PasswordConfirmed = null;
      IsBusy = false;
    }
    catch (Exception ex)
    {
      IsBusy = false;
      Logger.Info("Sign up failed: {Exception}", ex);
      await Notifier.ShowAlertAsync("Sign up failed", ex.Message, "OK");
    }
  }

  [RelayCommand]
  private async Task SignIn(ITextInput view)
  {
    if (!IsInputValid())
      return;

    try
    {
      IsBusy = true;
      UserEmail = UserEmail!.ToLower().Trim();
      await RealmService.SignInAsync(UserEmail, Password!);
      HideKeyboard(view);
      await Shell.Current.Navigation.PopAsync();
      IsUserSignedIn = true;
      Password = null;
    }
    catch (Exception ex)
    {
      Logger.Info("Sign in failed: {Exception}", ex);
      await RealmService.SignOutAsync();
      await Notifier.ShowAlertAsync("Sign in failed", ex.Message, "OK");
    }
    finally
    {
      IsBusy = false;
    }
  }

  // TODO: Add password reset functionality (requires sending emails though)
  [RelayCommand]
  private static async Task ForgotPassword()
  {
    await Notifier.ShowAlertAsync(
      "Password reset",
      "For security reasons, you cannot reset your password. This app gives you privacy by not requiring you to enter and then verify an email address. However, this comes at a cost: there is no way of verifying a user without their password.",
      "OK"
    );
  }

  [RelayCommand]
  private async Task GoToSignUp()
  {
    await Shell.Current.Navigation.PushAsync(_serviceProvider.GetService<SignUpPage>());
  }

  [RelayCommand]
  private async Task GoToSignIn()
  {
    await Shell.Current.Navigation.PushAsync(_serviceProvider.GetService<SignInPage>());
  }

  [RelayCommand]
  private static void SignOut()
  {
    RealmService.SignOutAsync().SafeFireAndForget();
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

  private bool IsInputValid()
  {
    if (string.IsNullOrEmpty(UserEmail))
    {
      Notifier.ShowToast("You must enter a user name");
      return false;
    }

    if (string.IsNullOrEmpty(Password))
    {
      Notifier.ShowToast("You must enter a password");
      return false;
    }

    if (UserEmail.Length < 6)
    {
      Notifier.ShowAlertAsync(
        "Invalid user name",
        "User name must be at least 6 characters long.",
        "OK"
      );
      return false;
    }

    if (PasswordRequirementRegex().IsMatch(Password))
      return true;

    Notifier.ShowAlertAsync(
      "Invalid password",
      "Password must be at least 6 characters long, including a number, a letter, and a special character.",
      "OK"
    );
    return false;
  }

  [GeneratedRegex(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[!@#$%^&*]).{6,}$")]
  private static partial Regex PasswordRequirementRegex();

  // ReSharper disable once UnusedParameter.Local
  private void HideKeyboard(ITextInput view)
  {
#if __ANDROID__
    var isKeyboardHidden = view.HideKeyboardAsync(CancellationToken.None);
    Logger.Info("Keyboard hidden: {State}", isKeyboardHidden);
#endif
  }
}
