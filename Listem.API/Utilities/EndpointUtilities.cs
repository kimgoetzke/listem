using System.Security.Claims;
using Listem.API.Domain.Lists;
using Listem.API.Exceptions;

namespace Listem.API.Utilities;

public static class EndpointUtilities
{
    public static string GetUserForLoggedRequest(ClaimsPrincipal user, string message)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var email =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                ? user.FindFirst(ClaimTypes.Email)?.Value
                : "<Redacted>";

        if (userId is not null)
        {
            Logger.Log($"Request from {email}, id {userId}: {message}");
            return userId;
        }

        const string errorMessage = "User is not authenticated or cannot be identified";
        Logger.Log(errorMessage);
        throw new BadRequestException(errorMessage);
    }

    public static async Task ThrowIfListDoesNotExist(
        IListService listService,
        string userId,
        string listId
    )
    {
        if (!await listService.ExistsAsync(userId, listId))
        {
            throw new BadRequestException($"List {listId} does not exist");
        }
    }
}
