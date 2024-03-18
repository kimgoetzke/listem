namespace Listem.Mobile.Models;

public class User
{
    public const string AccessTokenName = "AuthenticationToken";
    public const string RefreshTokenName = "RefreshToken";
    public const string ExpiresInName = "ExpiresIn";
    public const string EmailAddressName = "EmailAddress";

    public bool IsRegistered => EmailAddress != null;
    public bool IsSignedIn => IsTokenExpired == false;
    public string? EmailAddress { get; set; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime TokenExpiresAt { get; init; } = DateTime.Now;
    public bool IsTokenExpired => TokenExpiresAt <= DateTime.Now;

    public override string ToString()
    {
        return $"[User] Email: {EmailAddress ?? "null"}, signed in: {IsSignedIn}, access token: {AccessToken?[..10] ?? "null"} (expired={IsTokenExpired}, {TokenExpiresAt}), refresh token: {RefreshToken?[..10] ?? "null"}";
    }
}
