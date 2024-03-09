using System.Security.Claims;
using Listem.API.Contracts;
using Listem.API.Domain.Items;
using Listem.API.Domain.Lists;
using Microsoft.AspNetCore.Mvc;
using static Listem.API.Utilities.EndpointUtilities;

namespace Listem.API.Domain.Categories;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("api/categories", GetAll).RequireAuthorization();
        var group = endpoints.MapGroup("api/lists");
        group.MapGet("{listId}/categories", GetAllByListId).RequireAuthorization();
        group.MapPost("{listId}/categories", Create).RequireAuthorization();
        group.MapPut("{listId}/categories/{id}", Update).RequireAuthorization();
        group.MapDelete("{listId}/categories/{id}", DeleteById).RequireAuthorization();
        group.MapDelete("{listId}/categories", ResetByListId).RequireAuthorization();
    }

    private static async Task<IResult> GetAll(
        ClaimsPrincipal user,
        ICategoryService categoryService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"GET all categories");
        var categories = await categoryService.GetAllAsync(userId);
        return Results.Ok(categories);
    }

    private static async Task<IResult> GetAllByListId(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        ICategoryService categoryService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"GET all categories for {listId}");
        var categories = await categoryService.GetAllByListIdAsync(userId, listId);
        return Results.Ok(categories);
    }

    private static async Task<IResult> Create(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        [FromBody] CategoryRequest category,
        ICategoryService categoryService,
        IListService listService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"CREATE {category} in {listId}");
        await ThrowIfListDoesNotExist(listService, userId, listId);
        var createdCategory = await categoryService.CreateAsync(userId, listId, category);
        return Results.Created(
            $"api/lists/{listId}/categories/{createdCategory!.Id}",
            createdCategory
        );
    }

    private static async Task<IResult> Update(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        [FromRoute] string id,
        [FromBody] CategoryRequest category,
        ICategoryService categoryService,
        IListService listService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"UPDATE category {id} in {listId}");
        await ThrowIfListDoesNotExist(listService, userId, listId);
        var updatedCategory = await categoryService.UpdateAsync(userId, listId, id, category);
        return Results.Ok(updatedCategory);
    }

    private static async Task<IResult> DeleteById(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        [FromRoute] string id,
        ICategoryService categoryService,
        IListService listService,
        IItemService itemService
    )
    {
        var userId = GetUserForLoggedRequest(user, $"DELETE category {id} in {listId}");
        await ThrowIfListDoesNotExist(listService, userId, listId);
        var defaultCategory = await categoryService.GetDefaultCategory(userId, listId);
        await UpdateItemsToDefaultCategory(itemService, listId, userId, defaultCategory.Id, id);
        await categoryService.DeleteByIdAsync(userId, listId, id);
        return Results.NoContent();
    }

    private static async Task<IResult> ResetByListId(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        ICategoryService categoryService,
        IListService listService,
        IItemService itemService
    )
    {
        var userId = GetUserForLoggedRequest(
            user,
            $"DELETE all categories except default in {listId}"
        );
        await ThrowIfListDoesNotExist(listService, userId, listId);
        var defaultCategory = await categoryService.GetDefaultCategory(userId, listId);
        await UpdateItemsToDefaultCategory(itemService, listId, userId, defaultCategory.Id);
        await categoryService.DeleteAllByListIdAsync(userId, listId, defaultCategory.Id);
        return Results.NoContent();
    }

    private static async Task UpdateItemsToDefaultCategory(
        IItemService itemService,
        string listId,
        string userId,
        string defaultCategoryId,
        string? currentCategoryId = null
    )
    {
        if (currentCategoryId is null)
        {
            await itemService.UpdateToDefaultCategoryAsync(userId, listId, defaultCategoryId);
            return;
        }
        await itemService.UpdateToDefaultCategoryAsync(
            userId,
            listId,
            defaultCategoryId,
            currentCategoryId
        );
    }
}
