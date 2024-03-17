namespace Listem.Contracts;

// ReSharper disable once ClassNeverInstantiated.Global
public record UserLoginResponse(
    string TokenType,
    string AccessToken,
    string RefreshToken,
    int ExpiresIn
);
