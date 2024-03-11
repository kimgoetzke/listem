using System.Net.Http.Headers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Utilities;
using Listem.Views;

namespace Listem.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string _emailAddress = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _passwordConfirmed = string.Empty;

    private readonly HttpClient _httpClient = new();

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private async Task Login()
    {
        if (Password != PasswordConfirmed)
        {
            Logger.Log("Passwords do not match!");
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
}
