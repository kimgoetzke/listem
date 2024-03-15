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
    public User CurrentUser { get; private set; } = null!;

    private readonly HttpClient _httpClient;

    private static JsonSerializerOptions JsonOptions =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        SignOut(); // TODO: Remove once auth is implemented
        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
        InitialiseComponent().SafeFireAndForget();
    }

    private async Task InitialiseComponent()
    {
        CurrentUser = await FetchUserFromStorage().ConfigureAwait(false) ?? new User();
        Logger.Log($"Initialised auth service with user: {CurrentUser.EmailAddress}");
    }

    public async Task<User?> Authenticate()
    {
        // TODO: Login, refresh token, etc.
        return await FetchUserFromStorage();
    }

    private static async Task<User?> FetchUserFromStorage()
    {
        var storedUser = await SecureStorage.Default.GetAsync("CurrentUser");
        if (storedUser == null)
            return null;

        var deserialisedUser = JsonSerializer.Deserialize<User>(storedUser);
        return deserialisedUser ?? null;
    }

    public async Task<AuthResult> Register(UserCredentials credentials)
    {
        var json = JsonSerializer.Serialize(credentials, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
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
            const string msg = "Error code RU1 - please notify the developer";
            Notifier.ShowToast(msg);
            return new AuthResult(false, msg);
        }
    }

    public async Task<AuthResult> Login(UserCredentials credentials)
    {
        var json = JsonSerializer.Serialize(credentials, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
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
            const string msg = "Error code LU2 - please notify the developer";
            return new AuthResult(false, msg);
        }
    }

    private static async Task<string> ParseErrorResponse(HttpResponseMessage response, string url)
    {
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Logger.Log($"Error response from {url}: {errorResponse}");
        var msg =
            errorResponse!.Errors?.Values.First().First() ?? "Failed to sign in - please try again";
        return msg;
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
            .Default.SetAsync(Constants.UserEmail, JsonSerializer.Serialize(email))
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
