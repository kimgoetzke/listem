using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Contracts;
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

    private static readonly HttpClient HttpClient =
        new(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(1) })
        {
            BaseAddress = new Uri("http://10.0.2.2:5041")
        };

    private static JsonSerializerOptions JsonOptions =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private async Task Login()
    {
        StopIfNull([EmailAddress, Password]);
        var json = JsonSerializer.Serialize(
            new UserCredentials(EmailAddress!, Password!),
            JsonOptions
        );
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await HttpClient.PostAsync("login", content);
            Logger.Log($"Responded {response.StatusCode} to POST /login: {response}");
        }
        catch (HttpRequestException e)
        {
            Logger.Log($"Responded {e.StatusCode} to POST /login: {e}");
            Notifier.ShowToast("Failed to connect - please try again later");
        }
    }

    [RelayCommand]
    private async Task Register()
    {
        StopIfNull([EmailAddress, Password, PasswordConfirmed]);

        if (Password != PasswordConfirmed)
        {
            Logger.Log("Passwords do not match!");
            Notifier.ShowToast("The passwords must match");
            return;
        }

        var json = JsonSerializer.Serialize(
            new UserCredentials(EmailAddress!, Password!),
            JsonOptions
        );
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await HttpClient.PostAsync("/register", content);
            Logger.Log($"Responded '{response.StatusCode}' to POST /register: {response}");
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                var errorObj = JsonSerializer.Deserialize<ErrorResponse>(
                    errorResponse,
                    JsonOptions
                );
                Logger.Log($"Error response: {errorResponse}");
                Notifier.ShowToast(
                    errorObj!.Errors?.Values.First().First()
                        ?? "Failed to connect - please try again later"
                );
            }
        }
        catch (HttpRequestException)
        {
            Notifier.ShowToast("Failed to connect - please notify the developer");
        }
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

        Notifier.ShowToast("You must enter your email and password first");
    }
}
