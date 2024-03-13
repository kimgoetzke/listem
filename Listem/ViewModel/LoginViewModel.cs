using System.Net.Http.Headers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Utilities;
using Listem.Views;

namespace Listem.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _emailAddress;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private string? _passwordConfirmed;

    private readonly HttpClient _httpClient = new();

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private async Task Login()
    {
        StopIfNull([EmailAddress, Password]);
    }

    [RelayCommand]
    private async Task Register()
    {
        StopIfNull([EmailAddress, Password, PasswordConfirmed]);

        if (Password != PasswordConfirmed)
        {
            Logger.Log("Passwords do not match!");
            Notifier.ShowToast("Passwords do not match.");
            return;
        }

        var json = $"{{\"email\": \"{EmailAddress}\", \"password\": \"{Password}\"}}";

        await _httpClient
            .PostAsync(
                "http://localhost:5555/login",
                new StringContent(json, MediaTypeHeaderValue.Parse("application/json"))
            )
            .ConfigureAwait(false);
    }

    [RelayCommand]
    private static async Task GoToSignUp()
    {
        await Shell.Current.Navigation.PushAsync(new SignUpPage());
    }

    private static void StopIfNull(IEnumerable<string> strings)
    {
        if (!strings.Any(string.IsNullOrEmpty))
            return;

        Notifier.ShowToast("You must enter your password and email first.");
    }
}
