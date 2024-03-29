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

  public void SignUp(string email)
  {
    EmailAddress = email;
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
    var at = ExtractPayload(AccessToken);
    var rt = ExtractPayload(RefreshToken);
    return $"[User] _id: {Id ?? "null"}, email: {EmailAddress ?? "null"}, status: {Status}, valid token: {!IsTokenExpired}, valid until: {TokenExpiresAt}), access token: {at ?? "null"}, refresh token: {rt ?? "null"}";
  }

  // Extracts the payload from a JWT token so that header and signature aren't logged.
  private static string? ExtractPayload(string? token)
  {
    if (token is null)
      return null;

    var parts = token.Split(".");
    return parts.Length != 3 ? token : parts[1];
  }
}

public enum Status
{
  NotSignedInAndNotRegistered,
  NotSignedInButRegistered,
  SignedIn
}
