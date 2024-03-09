using System.Security.Claims;
using Listem.API.Contracts;
using Listem.API.Domain.Lists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Listem.API.Utilities.EndpointUtilities;

namespace Listem.API.Domain.Items;

[Route("api/items")]
[ApiController]
public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("api/items", GetAll).RequireAuthorization();
        var group = endpoints.MapGroup("api/lists");
        group.MapGet("{listId}/items", GetAllByListId).RequireAuthorization();
        group.MapPost("{listId}/items", Create).RequireAuthorization();
        group.MapPut("{listId}/items/{id}", Update).RequireAuthorization();
        group.MapDelete("{listId}/items/{id}", Delete).RequireAuthorization();
        group.MapDelete("{listId}/items", DeleteAllByList).RequireAuthorization();
    }

    [HttpGet, Authorize]
    private static async Task<IResult> GetAll(ClaimsPrincipal user, IItemService itemService)
    {
        var userId = GetUserForLoggedRequest(user, "GET all items");
        var items = await itemService.GetAllAsync(userId);
        return Results.Ok(items);
    }

    [HttpGet("{listId}"), Authorize]
    private static async Task<IResult> GetAllByListId(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        IItemService itemService,
        IListService listService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"GET all items for list {listId}");
        await ThrowIfListDoesNotExist(listService, userId, listId);
        var items = await itemService.GetAllByListIdAsync(userId, listId);
        return Results.Ok(items);
    }

    [HttpPost("{listId}"), Authorize]
    private static async Task<IResult> Create(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        [FromBody] ItemRequest item,
        IItemService itemService,
        IListService listService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"CREATE {item} in list {listId}");
        await ThrowIfListDoesNotExist(listService, userId, listId);
        var createdItem = await itemService.CreateAsync(userId, listId, item);
        return Results.Created($"api/lists/{listId}/items/{createdItem!.Id}", createdItem);
    }

    [HttpPut("{listId}/{id}"), Authorize]
    private static async Task<IResult> Update(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        [FromRoute] string id,
        [FromBody] ItemRequest item,
        IItemService itemService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"UPDATE item {id} in list {listId}");
        var updatedItem = await itemService.UpdateAsync(userId, listId, id, item);
        return Results.Ok(updatedItem);
    }

    [HttpDelete("{listId}/{id}"), Authorize]
    private static async Task<IResult> Delete(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        [FromRoute] string id,
        IItemService itemService,
        IListService listService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"DELETE item {id} in list {listId}");
        await ThrowIfListDoesNotExist(listService, userId, listId);
        await itemService.DeleteByIdAsync(userId, listId, id);
        return Results.NoContent();
    }

    [HttpDelete("{listId}"), Authorize]
    private static async Task<IResult> DeleteAllByList(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        IItemService itemService,
        IListService listService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"DELETE all items in list {listId}");
        await ThrowIfListDoesNotExist(listService, userId, listId);
        await itemService.DeleteAllByListIdAsync(userId, listId);
        return Results.NoContent();
    }
}
