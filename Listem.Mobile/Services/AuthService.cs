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

    public bool IsUserSignedIn() => CurrentUser.IsSignedIn;

    private readonly HttpClient _httpClient;
    private readonly IConnectivity _connectivity;

    // TODO: Use Polly or equivalent to handle retries and timeouts
    public AuthService(IHttpClientFactory httpClientFactory, IConnectivity connectivity)
    {
        _connectivity = connectivity;
        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
        InitialiseComponent().SafeFireAndForget();
    }

    private async Task InitialiseComponent()
    {
        var user = await GetCurrentUser();
        if (user.IsTokenExpired)
        {
            var result = await RefreshToken();
            Logger.Log(result.Message);
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new UserIsSignedInMessage(user.IsSignedIn));
        }
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
            WeakReferenceMessenger.Default.Send(new UserIsSignedInMessage(false));
        }
    }

    public async Task<User> GetCurrentUser()
    {
        if (CurrentUser.IsRegistered)
            return CurrentUser;

        CurrentUser = await FetchUserFromStorage() ?? new User();
        return CurrentUser;
    }

    private static async Task<User?> FetchUserFromStorage()
    {
        if (await SecureStorage.Default.GetAsync(Constants.User) is not { } str)
            return null;

        var storedUser = JsonSerializer.Deserialize<StoredUser>(str);
        return storedUser != null ? User.From(storedUser) : null;
    }

    public async Task<HttpRequestResult> SignUp(UserCredentials credentials)
    {
        try
        {
            var content = Content(credentials);
            var response = await LoggedRequest(() => _httpClient.PostAsync("/register", content));
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
            const string msg = "Error code SU1 - try again or notify the developer";
            Notifier.ShowToast(msg);
            return new HttpRequestResult(false, msg);
        }
        catch (TaskCanceledException e)
        {
            Logger.Log($"Error: {e.Message}");
            return new HttpRequestResult(false, Constants.TimeoutMessage);
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

                if (message == Constants.UnauthorisedMessage)
                    message = "Invalid email and/or password - please try again";

                return new HttpRequestResult(false, message);
            }
            var loginResponse = await response.Content.ReadFromJsonAsync<UserLoginResponse>();
            UpdateCurrentUser(credentials.Email, loginResponse);
            return new HttpRequestResult(true, "Successfully signed in");
        }
        catch (HttpRequestException e)
        {
            Logger.Log($"Error: {e}");
            const string msg = "Error code LI2 - try again or notify the developer";
            return new HttpRequestResult(false, msg);
        }
        catch (TaskCanceledException e)
        {
            Logger.Log($"Error: {e.Message}");
            return new HttpRequestResult(false, Constants.TimeoutMessage);
        }
    }

    private async Task<HttpRequestResult> RefreshToken()
    {
        if (CurrentUser.RefreshToken == null)
            return new HttpRequestResult(false, "There is no refresh token to refresh the session");

        if (CurrentUser.EmailAddress == null)
            return new HttpRequestResult(false, "No user email set, won't refresh session");

        try
        {
            var content = Content(new { refreshToken = CurrentUser.RefreshToken });
            var response = await LoggedRequest(() => _httpClient.PostAsync("/refresh", content));
            if (!response.IsSuccessStatusCode)
            {
                var message = await ParseErrorResponse(response);

                if (message == Constants.DefaultMessage)
                    message = "Session is invalid - you need to sign in again";

                WeakReferenceMessenger.Default.Send(new UserIsSignedInMessage(false));
                return new HttpRequestResult(false, message);
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<UserLoginResponse>();
            UpdateCurrentUser(loginResponse: loginResponse);
            return new HttpRequestResult(true, "Successfully refreshed session");
        }
        catch (HttpRequestException e)
        {
            Logger.Log($"Error: {e}");
            const string msg = "Error code RT3 - try again or notify the developer";
            return new HttpRequestResult(false, msg);
        }
        catch (TaskCanceledException e)
        {
            Logger.Log($"Error: {e.Message}");
            return new HttpRequestResult(false, Constants.TimeoutMessage);
        }
    }

    private void UpdateCurrentUser(string? email = null, UserLoginResponse? loginResponse = null)
    {
        if (email != null)
        {
            CurrentUser = User.WithEmail(email);
            WeakReferenceMessenger.Default.Send(new UserEmailSetMessage(email));
        }

        if (loginResponse != null)
        {
            CurrentUser.SignIn(loginResponse);
            WeakReferenceMessenger.Default.Send(new UserIsSignedInMessage(true));
        }

        if (email == null && loginResponse == null)
        {
            CurrentUser.SignOut();
            WeakReferenceMessenger.Default.Send(new UserIsSignedInMessage(false));
        }

        SecureStorage
            .Default.SetAsync(Constants.User, JsonSerializer.Serialize(CurrentUser))
            .ConfigureAwait(false);
        Logger.Log($"Updated current user to: {CurrentUser}");
    }

    public void SignOut()
    {
        UpdateCurrentUser();
    }
}
