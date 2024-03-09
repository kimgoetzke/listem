using System.Security.Claims;
using Listem.API.Contracts;
using Listem.API.Domain.Categories;
using Listem.API.Domain.Items;
using Microsoft.AspNetCore.Mvc;
using static Listem.API.Utilities.EndpointUtilities;

namespace Listem.API.Domain.Lists;

public static class ListEndpoints
{
    public static void MapListEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("api/lists");
        group.MapGet("", GetAll).RequireAuthorization();
        group.MapGet("{id}", GetById).RequireAuthorization();
        group.MapPost("", Create).RequireAuthorization();
        group.MapPut("{id}", Update).RequireAuthorization();
        group.MapDelete("{id}", Delete).RequireAuthorization();
    }

    private static async Task<IResult> GetAll(ClaimsPrincipal user, IListService listService)
    {
        var userId = GetUserForLoggedRequest(user, "GET all lists");
        var itemLists = await listService.GetAllAsync(userId);
        return Results.Ok(itemLists);
    }

    private static async Task<IResult> GetById(
        ClaimsPrincipal user,
        [FromRoute] string id,
        IListService listService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"GET list {id}");
        var itemList = await listService.GetByIdAsync(userId, id);
        return Results.Ok(itemList);
    }

    private static async Task<IResult> Create(
        ClaimsPrincipal user,
        [FromBody] ListRequest list,
        IListService listService,
        ICategoryService categoryService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"CREATE {list}");
        var createdItemList = await listService.CreateAsync(userId, list);
        var defaultCategory = new CategoryRequest { Name = ICategoryService.DefaultCategoryName };
        await categoryService.CreateAsync(userId, createdItemList!.Id, defaultCategory);
        return Results.Created($"api/lists/{createdItemList.Id}", createdItemList);
    }

    private static async Task<IResult> Update(
        ClaimsPrincipal user,
        [FromRoute] string id,
        [FromBody] ListRequest list,
        IListService listService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"UPDATE list {id}");
        var updatedItemList = await listService.UpdateAsync(userId, id, list);
        return Results.Ok(updatedItemList);
    }

    private static async Task<IResult> Delete(
        ClaimsPrincipal user,
        [FromRoute] string id,
        IListService listService,
        ICategoryService categoryService,
        IItemService itemService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"DELETE list {id}");
        await ThrowIfListDoesNotExist(listService, userId, id);
        await categoryService.DeleteAllByListIdAsync(userId, id);
        await itemService.DeleteAllByListIdAsync(userId, id);
        await listService.DeleteByIdAsync(userId, id);
        return Results.NoContent();
    }
}
