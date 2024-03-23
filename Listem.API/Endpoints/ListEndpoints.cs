using Listem.API.Domain.Categories;
using Listem.API.Domain.Items;
using Listem.API.Domain.Lists;
using Listem.API.Middleware;
using Listem.Shared.Contracts;
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

    private static async Task<IResult> GetAll(IListService listService)
    {
        var itemLists = await listService.GetAllAsync();
        return Results.Ok(itemLists);
    }

    private static async Task<IResult> GetById([FromRoute] string id, IListService listService)
    {
        var itemList = await listService.GetByIdAsync(id);
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
        var defaultCategory = new CategoryRequest { Name = Shared.Constants.DefaultCategoryName };
        await categoryService.CreateAsync(req.UserId, createdItemList!.Id, defaultCategory);
        return Results.Created($"api/lists/{createdItemList.Id}", createdItemList);
    }

    private static async Task<IResult> Update(
        [FromRoute] string id,
        [FromBody] ListRequest list,
        IListService listService
    )
    {
        var updatedItemList = await listService.UpdateAsync(id, list);
        return Results.Ok(updatedItemList);
    }

    private static async Task<IResult> Delete(
        [FromRoute] string id,
        IListService listService,
        ICategoryService categoryService,
        IItemService itemService
    )
    {
        await ThrowIfListDoesNotExist(listService, id);
        await itemService.DeleteAllByListIdAsync(id);
        await categoryService.DeleteAllByListIdAsync(id);
        await listService.DeleteByIdAsync(id);
        return Results.NoContent();
    }
}
