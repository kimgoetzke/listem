namespace Listem.Mobile.Models;

public class User
{
    public string? Id { get; set; }
    public string? EmailAddress { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    private DateTime TokenExpiresAt { get; set; } = DateTime.Now;
    public bool IsTokenExpired => TokenExpiresAt <= DateTime.Now;
    public bool IsRegistered => EmailAddress != null;
    public bool IsSignedIn => IsTokenExpired == false;
    public Status Status
    {
        get
        {
            return IsSignedIn switch
            {
                true => Status.SignedIn,
                false when IsRegistered => Status.NotSignedInButRegistered,
                _ => Status.NotSignedInAndNotRegistered
            };
        }
    }

    public void SignUp(Realms.Sync.User realmUser)
    {
        Id = realmUser.Id;
        EmailAddress = realmUser.Profile.Email;
        AccessToken = null;
        RefreshToken = null;
        TokenExpiresAt = DateTime.Now;
    }

    public void Update(Realms.Sync.User realmUser)
    {
        if (realmUser.Profile.Email != null && realmUser.Profile.Email != EmailAddress)
        {
            Id = realmUser.Id;
            EmailAddress = realmUser.Profile.Email;
        }
        AccessToken = realmUser.AccessToken;
        RefreshToken = realmUser.RefreshToken;
        TokenExpiresAt = DateTime.Now + TimeSpan.FromDays(29);
    }

    public void SignIn(Realms.Sync.User realmUser)
    {
        if (realmUser.Profile.Email != null && realmUser.Profile.Email != EmailAddress)
        {
            Id = realmUser.Id;
            EmailAddress = realmUser.Profile.Email;
        }
        AccessToken = realmUser.AccessToken;
        RefreshToken = realmUser.RefreshToken;
        TokenExpiresAt = DateTime.Now + TimeSpan.FromDays(29);
    }

    public void SignOut()
    {
        AccessToken = null;
        RefreshToken = null;
        TokenExpiresAt = DateTime.Now;
    }

    public override string ToString()
    {
        var accessTokenPayload = AccessToken?.Split(".")[1];
        var refreshTokenPayload = RefreshToken?.Split(".")[1];
        return $"[User] Email: {EmailAddress ?? "null"}, signed in: {IsSignedIn}, access token: {accessTokenPayload ?? "null"} (valid={!IsTokenExpired}, until={TokenExpiresAt}), refresh token: {refreshTokenPayload ?? "null"}";
    }
}

public enum Status
{
    NotSignedInAndNotRegistered,
    NotSignedInButRegistered,
    SignedIn
}
