namespace Listem.Mobile.Models;

public class User
{
    public const string AccessTokenName = "AuthenticationToken";
    public const string RefreshTokenName = "RefreshToken";
    public const string ExpiresInName = "ExpiresIn";
    public const string EmailAddressName = "EmailAddress";

    public bool HasRegistered => EmailAddress != null;
    public bool IsAuthenticated { get; set; }
    public string? EmailAddress { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int TokenExpiresIn { get; set; }
    public string? TokenType { get; set; }

    public override string ToString()
    {
        return $"[User] Email: {EmailAddress}, authenticated: {IsAuthenticated}, access token: {AccessToken?[..10]} (expires in {TokenExpiresIn} seconds), refresh token: {RefreshToken?[..10]}";
    }
}
