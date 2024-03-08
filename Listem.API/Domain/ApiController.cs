using System.Security.Claims;
using Listem.API.Domain.Lists;
using Listem.API.Exceptions;
using Listem.API.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Listem.API.Domain;

[ApiController]
public class ApiController(IListService listService) : ControllerBase
{
    protected string ValidateUserRequestOrThrow(string message)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                ? User.FindFirst(ClaimTypes.Email)?.Value
                : userId;

        if (userId is not null)
        {
            Logger.Log($"Request from {user}: {message}");
            return userId;
        }

        const string errorMessage = "User is not authenticated or cannot be identified";
        Logger.Log(errorMessage);
        throw new BadRequestException(errorMessage);
    }

    protected async Task ThrowIfListDoesNotExist(string userId, string listId)
    {
        if (!await listService.ExistsAsync(userId, listId))
        {
            throw new BadRequestException($"List {listId} does not exist");
        }
    }
}
