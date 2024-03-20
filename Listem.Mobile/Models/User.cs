using System.Text.Json.Serialization;
using Listem.Shared.Contracts;

namespace Listem.Mobile.Models;

public class User
{
    public string? EmailAddress { get; private init; }
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    private DateTime TokenExpiresAt { get; set; } = DateTime.Now;
    public bool IsTokenExpired => TokenExpiresAt <= DateTime.Now;
    public bool IsRegistered => EmailAddress != null;
    public bool IsSignedIn => IsTokenExpired == false;

    public static User From(StoredUser storedUser)
    {
        return new User
        {
            EmailAddress = storedUser.EmailAddress,
            AccessToken = storedUser.AccessToken,
            RefreshToken = storedUser.RefreshToken,
            TokenExpiresAt = storedUser.TokenExpiresAt
        };
    }

    public static User WithEmail(string emailAddress)
    {
        return new User { EmailAddress = emailAddress };
    }

    public void SignIn(UserLoginResponse? loginResponse)
    {
        if (EmailAddress == null)
            throw new InvalidOperationException("Cannot sign in a user without an email address");

        AccessToken = loginResponse?.AccessToken;
        RefreshToken = loginResponse?.RefreshToken;
        TokenExpiresAt = DateTime.Now.AddSeconds(loginResponse?.ExpiresIn ?? -1);
    }

    public void SignOut()
    {
        AccessToken = null;
        RefreshToken = null;
        TokenExpiresAt = DateTime.Now;
    }

    public override string ToString()
    {
        return $"[User] Email: {EmailAddress ?? "null"}, signed in: {IsSignedIn}, access token: {AccessToken?[..10] ?? "null"} (valid={!IsTokenExpired}, {TokenExpiresAt}), refresh token: {RefreshToken?[..10] ?? "null"}";
    }
}

public class StoredUser
{
    public string? EmailAddress { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime TokenExpiresAt { get; init; }
}
