using Listem.API.Contracts;
using Listem.API.Domain.Categories;
using Listem.API.Domain.Items;
using Listem.API.Domain.Lists;
using Listem.API.Middleware;
using Microsoft.AspNetCore.Mvc;
using static Listem.API.Utilities.EndpointUtilities;

namespace Listem.API.Endpoints;

public static class ListEndpoints
{
    public static void MapListEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("api/lists", GetAll).RequireAuthorization();
        endpoints.MapGet("api/lists/{id}", GetById).RequireAuthorization();
        endpoints.MapPost("api/lists", Create).RequireAuthorization();
        endpoints.MapPut("api/lists/{id}", Update).RequireAuthorization();
        endpoints.MapDelete("api/lists/{id}", Delete).RequireAuthorization();
    }

    private static async Task<IResult> GetAll(IRequestContext req, IListService listService)
    {
        var itemLists = await listService.GetAllAsync(req.UserId);
        return Results.Ok(itemLists);
    }

    private static async Task<IResult> GetById(
        IRequestContext req,
        [FromRoute] string id,
        IListService listService
    )
    {
        var itemList = await listService.GetByIdAsync(req.UserId, id);
        return Results.Ok(itemList);
    }

    private static async Task<IResult> Create(
        IRequestContext req,
        [FromBody] ListRequest list,
        IListService listService,
        ICategoryService categoryService
    )
    {
        var createdItemList = await listService.CreateAsync(req.UserId, list);
        var defaultCategory = new CategoryRequest { Name = ICategoryService.DefaultCategoryName };
        await categoryService.CreateAsync(req.UserId, createdItemList!.Id, defaultCategory);
        return Results.Created($"api/lists/{createdItemList.Id}", createdItemList);
    }

    private static async Task<IResult> Update(
        IRequestContext req,
        [FromRoute] string id,
        [FromBody] ListRequest list,
        IListService listService
    )
    {
        var updatedItemList = await listService.UpdateAsync(req.UserId, id, list);
        return Results.Ok(updatedItemList);
    }

    private static async Task<IResult> Delete(
        IRequestContext req,
        [FromRoute] string id,
        IListService listService,
        ICategoryService categoryService,
        IItemService itemService
    )
    {
        await ThrowIfListDoesNotExist(listService, req.UserId, id);
        await itemService.DeleteAllByListIdAsync(req.UserId, id);
        await categoryService.DeleteAllByListIdAsync(req.UserId, id);
        await listService.DeleteByIdAsync(req.UserId, id);
        return Results.NoContent();
    }
}
