using System.Net.Http.Json;
using System.Text.Json;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Listem.Shared.Contracts;
using static Listem.Mobile.Utilities.HttpUtilities;

namespace Listem.Mobile.Services;

public class AuthService
{
    public User CurrentUser { get; private set; } = new();

    public bool IsOnline() => _connectivity.NetworkAccess == NetworkAccess.Internet;

    // TODO: Set this correctly when app starts
    public bool IsUserSignedIn() => CurrentUser.IsAuthenticated;

    private readonly HttpClient _httpClient;
    private readonly IConnectivity _connectivity;

    // TODO: Implement sign in using refresh token
    public AuthService(IHttpClientFactory httpClientFactory, IConnectivity connectivity)
    {
        _connectivity = connectivity;
        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
        InitialiseComponent().SafeFireAndForget();
    }

    private async Task InitialiseComponent()
    {
        CurrentUser = await FetchExistingUser().ConfigureAwait(false) ?? new User();
        Logger.Log($"Initialised auth service with user: {CurrentUser}");
    }

    public async Task NotifyIfNotOnline()
    {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await Shell.Current.DisplayAlert(
                "Offline mode",
                "Seems like you're offline. You can continue to use the app but you cannot use sharing features or backup your list in the cloud.",
                "OK"
            );
        }
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

    public async Task<HttpRequestResult> SignUp(UserCredentials credentials)
    {
        try
        {
            var response = await LoggedRequest(
                () => _httpClient.PostAsync("/register", Content(credentials))
            );
            if (!response.IsSuccessStatusCode)
            {
                var msg = await ParseErrorResponse(response);
                return new HttpRequestResult(false, msg);
            }
            UpdateCurrentUser(credentials.Email);
            return new HttpRequestResult(true, "Successfully registered");
        }
        catch (HttpRequestException e)
        {
            Logger.Log($"Error: {e}");
            const string msg = "Error code RU1 - try again or notify the developer";
            Notifier.ShowToast(msg);
            return new HttpRequestResult(false, msg);
        }
    }

    public async Task<HttpRequestResult> SignIn(UserCredentials credentials)
    {
        try
        {
            var content = Content(credentials);
            var response = await LoggedRequest(() => _httpClient.PostAsync("/login", content));
            if (!response.IsSuccessStatusCode)
            {
                var message = await ParseErrorResponse(response);
                if (message == "Unauthorized")
                    message = "Invalid email and/or password - please try again";
                return new HttpRequestResult(false, message);
            }
            var loginResponse = await response.Content.ReadFromJsonAsync<UserLoginResponse>();
            UpdateCurrentUser(credentials.Email, loginResponse!);
            WeakReferenceMessenger.Default.Send(new UserIsSignedInMessage(true));
            return new HttpRequestResult(true, "Successfully signed in");
        }
        catch (HttpRequestException e)
        {
            Logger.Log($"Error: {e}");
            const string msg = "Error code LU2 - try again or notify the developer";
            return new HttpRequestResult(false, msg);
        }
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
            .ConfigureAwait(false);
        WeakReferenceMessenger.Default.Send(new UserEmailSetMessage(email));
        Logger.Log($"Updated current user to: {CurrentUser}");
    }

    public void SignOut()
    {
        CurrentUser = new User();
        WeakReferenceMessenger.Default.Send(new UserIsSignedInMessage(false));
        SecureStorage.Default.RemoveAll();
    }
}
