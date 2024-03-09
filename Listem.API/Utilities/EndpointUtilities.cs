using System.Security.Claims;
using Listem.API.Domain.Lists;
using Listem.API.Exceptions;

namespace Listem.API.Utilities;

public static class EndpointUtilities
{
    public static string GetUserForLoggedRequest(ClaimsPrincipal user, string message)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)!.Value;
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
