using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Listem.Mobile.Views;
using Listem.Shared.Contracts;

namespace Listem.Mobile.ViewModel;

public partial class LoginViewModel : ObservableObject
{
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

        WeakReferenceMessenger.Default.Register<UserEmailSetMessage>(
            this,
            (_, m) =>
            {
                Logger.Log(
                    $"Received message: Setting current user email to '{m.Value}' in LoginViewModel"
                );
                Email = m.Value;
            }
        );
    }

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private async Task SignUp()
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
            await Shell.Current.Navigation.PopAsync();
            Password = null;
            PasswordConfirmed = null;
        }
    }

    [RelayCommand]
    private async Task SignIn()
    {
        var isValidated = ValidateInputFields([Email, Password]);
        if (!isValidated)
            return;

        var result = await _authService.SignIn(new UserCredentials(Email!, Password!));
        Notifier.ShowToast(result.Message);
        if (result.Success)
        {
            await Shell.Current.Navigation.PopAsync();
            Password = null;
        }
    }

    [RelayCommand]
    private async Task GoToSignUp()
    {
        var signUpPage = _serviceProvider.GetService<SignUpPage>();
        await Shell.Current.Navigation.PushAsync(signUpPage);
    }

    // TODO: Replace with real validation and visible requirements and live feedback on page
    private static bool ValidateInputFields(IEnumerable<string?> strings)
    {
        if (!strings.Any(string.IsNullOrEmpty))
            return true;

        Notifier.ShowToast("You must enter both email and password");
        return false;
    }
}
