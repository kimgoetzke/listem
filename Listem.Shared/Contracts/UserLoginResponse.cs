namespace Listem.Shared.Contracts;

// ReSharper disable once NotAccessedPositionalProperty.Global
// ReSharper disable once ClassNeverInstantiated.Global
public record UserLoginResponse(
    string TokenType,
    string AccessToken,
    string RefreshToken,
    int ExpiresIn
);
