using System.Security.Claims;
using Listem.API.Utilities;

namespace Listem.API.Middleware;

public interface IRequestContext
{
    public string? UserId { get; }
    public string? UserEmail { get; }
    public string? Role { get; }
    public string? RequestId { get; }
    public void Set(ClaimsPrincipal user);
}

public class RequestContext : IRequestContext
{
    public string? UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public string? Role { get; private set; }
    public string? RequestId { get; private set; }

    public void Set(ClaimsPrincipal user)
    {
        if (user is null)
        {
            throw new SystemException("User is not authenticated or cannot be identified");
        }

        UserId = user.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        UserEmail =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                ? user.FindFirst(ClaimTypes.Email)!.Value
                : "<Redacted>";
        Role = user.FindFirst(ClaimTypes.Role)?.Value;
        RequestId = IdProvider.NewId(nameof(RequestContext));
    }
}
