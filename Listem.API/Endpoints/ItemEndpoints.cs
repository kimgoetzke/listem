using Listem.API.Contracts;
using Listem.API.Domain.Items;
using Listem.API.Domain.Lists;
using Listem.API.Middleware;
using Microsoft.AspNetCore.Mvc;
using static Listem.API.Utilities.EndpointUtilities;

namespace Listem.API.Endpoints;

[Route("api/items")]
[ApiController]
public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("api/items", GetAll).RequireAuthorization();
        endpoints.MapGet("api/lists/{listId}/items", GetAllByListId).RequireAuthorization();
        endpoints.MapPost("api/lists/{listId}/items", Create).RequireAuthorization();
        endpoints.MapPut("api/lists/{listId}/items/{id}", Update).RequireAuthorization();
        endpoints.MapDelete("api/lists/{listId}/items/{id}", Delete).RequireAuthorization();
        endpoints.MapDelete("api/lists/{listId}/items", DeleteAllByList).RequireAuthorization();
    }

    private static async Task<IResult> GetAll(IItemService itemService)
    {
        var items = await itemService.GetAllAsync();
        return Results.Ok(items);
    }

    private static async Task<IResult> GetAllByListId(
        [FromRoute] string listId,
        IItemService itemService,
        IListService listService
    )
    {
        await ThrowIfListDoesNotExist(listService, listId);
        var items = await itemService.GetAllByListIdAsync(listId);
        return Results.Ok(items);
    }

    private static async Task<IResult> Create(
        IRequestContext req,
        [FromRoute] string listId,
        [FromBody] ItemRequest item,
        IItemService itemService,
        IListService listService
    )
    {
        await ThrowIfListDoesNotExist(listService, listId);
        var createdItem = await itemService.CreateAsync(req.UserId, listId, item);
        return Results.Created($"api/lists/{listId}/items/{createdItem!.Id}", createdItem);
    }

    private static async Task<IResult> Update(
        [FromRoute] string listId,
        [FromRoute] string id,
        [FromBody] ItemRequest item,
        IItemService itemService
    )
    {
        var updatedItem = await itemService.UpdateAsync(listId, id, item);
        return Results.Ok(updatedItem);
    }

    private static async Task<IResult> Delete(
        [FromRoute] string listId,
        [FromRoute] string id,
        IItemService itemService,
        IListService listService
    )
    {
        await ThrowIfListDoesNotExist(listService, listId);
        await itemService.DeleteByIdAsync(listId, id);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAllByList(
        [FromRoute] string listId,
        IItemService itemService,
        IListService listService
    )
    {
        await ThrowIfListDoesNotExist(listService, listId);
        await itemService.DeleteAllByListIdAsync(listId);
        return Results.NoContent();
    }
}
