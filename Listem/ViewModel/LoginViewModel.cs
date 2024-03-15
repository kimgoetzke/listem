using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Contracts;
using Listem.Events;
using Listem.Services;
using Listem.Utilities;
using Listem.Views;

namespace Listem.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private string? _passwordConfirmed;

    private readonly AuthService _authService;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;

        WeakReferenceMessenger.Default.Register<UserEmailSetMessage>(
            this,
            (_, m) =>
            {
                Logger.Log($"Received message: Setting current user email to '{m.Value}'");
                Email = m.Value;
                OnPropertyChanged(nameof(Email)); // TODO: Find out why email isn't pre-populated after first sign-in
            }
        );
    }

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private async Task Register()
    {
        StopIfNull([Email, Password, PasswordConfirmed]);

        if (Password != PasswordConfirmed)
        {
            const string msg = "Passwords do not match";
            Logger.Log(msg);
            Notifier.ShowToast(msg);
            return;
        }

        var result = await _authService.Register(new UserCredentials(Email!, Password!));
        Notifier.ShowToast(result.Message);
        if (result.Success)
        {
            await Shell.Current.Navigation.PopAsync();
        }
    }

    [RelayCommand]
    private async Task Login()
    {
        StopIfNull([Email, Password]);
        var result = await _authService.Login(new UserCredentials(Email!, Password!));
        Notifier.ShowToast(result.Message);
        if (result.Success)
        {
            await Shell.Current.Navigation.PopAsync();
        }
    }

    [RelayCommand]
    private static async Task GoToSignUp()
    {
        await Shell.Current.Navigation.PushAsync(new SignUpPage());
    }

    private static void StopIfNull(IEnumerable<string?> strings)
    {
        if (!strings.Any(string.IsNullOrEmpty))
            return;

        Notifier.ShowToast("You must enter your email and password first");
    }
}
