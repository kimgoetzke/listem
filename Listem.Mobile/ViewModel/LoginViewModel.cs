using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Listem.Mobile.Views;
using Listem.Shared.Contracts;
#if __ANDROID__
using CommunityToolkit.Maui.Core.Platform;
#endif

namespace Listem.Mobile.ViewModel;

public partial class LoginViewModel : ObservableObject
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
    private readonly AuthService _authService;

    // TODO: Enable displaying a loading state on app load (i.e. while attempting to refresh token)
    public LoginViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _authService =
            serviceProvider.GetService<AuthService>()
            ?? throw new NullReferenceException("AuthenticationService is null");
        Initialise();
    }

    private async void Initialise()
    {
        WeakReferenceMessenger.Default.Register<UserStatusChangedMessage>(
            this,
            (_, m) =>
            {
                Logger.Log(
                    $"[LoginViewModel] Received message: Current user status has changed to: {m.Value}"
                );
                UpdateUser(m.Value);
            }
        );

        var user = await _authService.GetCurrentUser();
        UpdateUser(user);
    }

    private void UpdateUser(User user)
    {
        UserStatus = user.Status;
        IsUserRegistered = user.IsRegistered;
        IsUserSignedIn = user.IsSignedIn;
        UserEmail = user.EmailAddress;
    }

    public void RedirectIfUserIsSignedIn()
    {
        if (!IsUserSignedIn)
            return;

        Logger.Log("User is signed in, redirecting now...");
        Shell.Current.Navigation.PushAsync(_serviceProvider.GetService<MainPage>());
    }

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private async Task SignUp(ITextInput view)
    {
        if (!IsInputValid([UserEmail, Password, PasswordConfirmed]))
            return;

        if (Password != PasswordConfirmed)
        {
            const string msg = "Passwords do not match";
            Logger.Log(msg);
            Notifier.ShowToast(msg);
            return;
        }

        var result = await _authService.SignUp(new UserCredentials(UserEmail!, Password!));
        Notifier.ShowToast(result.Message);
        if (result.Success)
        {
            HideKeyboard(view);
            await Shell.Current.Navigation.PopAsync();
            Password = null;
            PasswordConfirmed = null;
        }
    }

    [RelayCommand]
    private async Task SignIn(ITextInput view)
    {
        if (!IsInputValid([UserEmail, Password]))
            return;

        var result = await _authService.SignIn(new UserCredentials(UserEmail!, Password!));
        Notifier.ShowToast(result.Message);
        if (result.Success)
        {
            HideKeyboard(view);
            Password = null;
            await Shell.Current.Navigation.PopAsync();
        }
    }

    [RelayCommand]
    private static Task ForgotPassword()
    {
        Notifier.ShowToast("Sorry, not implemented yet");
        return Task.CompletedTask;
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
    private Task SignOut()
    {
        _authService.SignOut();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task GoToMainPage()
    {
        await Shell.Current.Navigation.PushAsync(_serviceProvider.GetService<MainPage>());
    }

    // TODO: Replace with real validation and add requirements and live feedback on page
    private static bool IsInputValid(IEnumerable<string?> strings)
    {
        if (!strings.Any(string.IsNullOrEmpty))
            return true;

        Notifier.ShowToast("You must enter both email and password");
        return false;
    }

    // ReSharper disable once UnusedParameter.Local
    private static void HideKeyboard(ITextInput view)
    {
#if __ANDROID__
        var isKeyboardHidden = view.HideKeyboardAsync(CancellationToken.None);
        Logger.Log("Keyboard hidden: " + isKeyboardHidden);
#endif
    }
}
