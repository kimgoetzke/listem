using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Contracts;
using Listem.Events;
using Listem.Models;
using Listem.Utilities;

namespace Listem.Services;

public class AuthService
{
    private User CurrentUser { get; set; } = null!;

    private readonly HttpClient _httpClient;
    private readonly IConnectivity _connectivity;

    private static JsonSerializerOptions JsonOptions =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

    // TODO: Implement sign in using refresh token
    public AuthService(IHttpClientFactory httpClientFactory, IConnectivity connectivity)
    {
        // SignOut(); // TODO: Remove once testing is done
        _connectivity = connectivity;
        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
        InitialiseComponent().SafeFireAndForget();
    }

    private async Task InitialiseComponent()
    {
        CurrentUser = await FetchExistingUser().ConfigureAwait(false) ?? new User();
        Logger.Log($"Initialised auth service with user: {CurrentUser}");
    }

    public async Task<bool> IsOnline()
    {
        return _connectivity.NetworkAccess == NetworkAccess.Internet;
        // if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        // {
        //     await Shell.Current.DisplayAlert(
        //         "Offline mode",
        //         "Seems like you're offline. You can continue to use the app but you cannot use sharing features or backup your list in the cloud.",
        //         "OK"
        //     );
        // }
    }

    public async Task<User?> FetchExistingUser()
    {
        // TODO: Login, refresh token, etc.
        return await FetchUserFromStorage();
    }

    private static async Task<User?> FetchUserFromStorage()
    {
        var storedUser = await SecureStorage.Default.GetAsync(Constants.User);

        if (storedUser == null)
            return null;

        return JsonSerializer.Deserialize<User>(storedUser) ?? null;
    }

    public async Task<AuthResult> SignUp(UserCredentials credentials)
    {
        try
        {
            var content = CreateStringContent(credentials);
            var response = await _httpClient.PostAsync("/register", content);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await ParseErrorResponse(response, "/register");
                return new AuthResult(false, msg);
            }
            Logger.Log($"Responded '{response.StatusCode}' to POST /register: {response}");
            UpdateCurrentUser(credentials.Email);
            return new AuthResult(true, "Successfully registered");
        }
        catch (HttpRequestException e)
        {
            Logger.Log($"Error: {e}");
            const string msg = "Error code RU1 - try again or notify the developer";
            Notifier.ShowToast(msg);
            return new AuthResult(false, msg);
        }
    }

    public async Task<AuthResult> SignIn(UserCredentials credentials)
    {
        try
        {
            var content = CreateStringContent(credentials);
            var response = await _httpClient.PostAsync("/login", content);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await ParseErrorResponse(response, "/login");
                return new AuthResult(false, msg);
            }
            var loginResponse = await response.Content.ReadFromJsonAsync<UserLoginResponse>();
            Logger.Log($"Responded '{response.StatusCode}' to POST /login: {loginResponse}");
            UpdateCurrentUser(credentials.Email, loginResponse!);
            return new AuthResult(true, "Successfully signed in");
        }
        catch (HttpRequestException e)
        {
            Logger.Log($"Error: {e}");
            const string msg = "Error code LU2 - try again or notify the developer";
            return new AuthResult(false, msg);
        }
    }

    private static StringContent CreateStringContent(UserCredentials credentials)
    {
        var json = JsonSerializer.Serialize(credentials, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<string> ParseErrorResponse(HttpResponseMessage response, string url)
    {
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Logger.Log($"Error response from {url}: {errorResponse}");
        return errorResponse!.Errors?.Values.First().First()
            ?? "Failed to sign in - please try again";
    }

    private void UpdateCurrentUser(string email, UserLoginResponse? loginResponse = null)
    {
        CurrentUser = new User
        {
            IsAuthenticated = loginResponse?.AccessToken != null,
            AccessToken = loginResponse?.AccessToken,
            RefreshToken = loginResponse?.RefreshToken,
            TokenExpiresIn = loginResponse?.ExpiresIn ?? -1,
            EmailAddress = email
        };
        SecureStorage
            .Default.SetAsync(Constants.User, JsonSerializer.Serialize(CurrentUser))
            .SafeFireAndForget();
        WeakReferenceMessenger.Default.Send(new UserEmailSetMessage(email));
        Logger.Log($"Updated current user to: {CurrentUser}");
    }

    public void SignOut()
    {
        CurrentUser = new User();
        SecureStorage.Default.RemoveAll();
    }
}
