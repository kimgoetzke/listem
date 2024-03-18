using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
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
    private string? _email;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private string? _passwordConfirmed;

    private readonly IServiceProvider _serviceProvider;
    private readonly AuthService _authService;

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
        WeakReferenceMessenger.Default.Register<UserEmailSetMessage>(
            this,
            (_, m) =>
            {
                Logger.Log(
                    $"Received message: Setting current user email to '{m.Value}' in LoginViewModel"
                );
                Email = m.Value;
                IsUserRegistered = true;
            }
        );

        WeakReferenceMessenger.Default.Register<UserIsSignedInMessage>(
            this,
            (_, m) =>
            {
                Logger.Log(
                    $"Received message: Current user sign in state set to '{m.Value}' in LoginViewModel"
                );
                IsUserSignedIn = m.Value;
            }
        );

        var user = await _authService.GetCurrentUser();
        IsUserRegistered = user.IsRegistered;
        IsUserSignedIn = user.IsSignedIn;
        Email = user.EmailAddress;
    }

    public void RedirectIfUserIsSignedIn()
    {
        if (IsUserSignedIn)
        {
            Shell.Current.Navigation.PushAsync(_serviceProvider.GetService<MainPage>());
        }
    }

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private async Task SignUp(ITextInput view)
    {
        var isValidated = ValidateInputFields([Email, Password, PasswordConfirmed]);
        if (!isValidated)
            return;

        if (Password != PasswordConfirmed)
        {
            const string msg = "Passwords do not match";
            Logger.Log(msg);
            Notifier.ShowToast(msg);
            return;
        }

        var result = await _authService.SignUp(new UserCredentials(Email!, Password!));
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
        var isValidated = ValidateInputFields([Email, Password]);
        if (!isValidated)
            return;

        var result = await _authService.SignIn(new UserCredentials(Email!, Password!));
        Notifier.ShowToast(result.Message);
        if (result.Success)
        {
            HideKeyboard(view);
            Password = null;
            await Shell.Current.Navigation.PopAsync();
        }
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
    private async Task LaunchInOfflineMode()
    {
        await Shell.Current.Navigation.PushAsync(_serviceProvider.GetService<MainPage>());
    }

    [RelayCommand]
    private async Task LaunchInOnlineMode()
    {
        await Shell.Current.Navigation.PushAsync(_serviceProvider.GetService<MainPage>());
    }

    // TODO: Replace with real validation and visible requirements and live feedback on page
    private static bool ValidateInputFields(IEnumerable<string?> strings)
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
