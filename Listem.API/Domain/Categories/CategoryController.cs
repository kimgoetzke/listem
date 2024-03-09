using System.Security.Claims;
using Listem.API.Contracts;
using Listem.API.Domain.Items;
using Listem.API.Domain.Lists;
using Listem.API.Exceptions;
using Listem.API.Utilities;
using Microsoft.AspNetCore.Mvc;

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

    public static async Task<IResult> GetAll(ClaimsPrincipal user, ICategoryService categoryService)
    {
        var userId = GetUserAndLog(user, $"{nameof(Category)} {nameof(GetAll)}");
        var categories = await categoryService.GetAllAsync(userId);
        return Results.Ok(categories);
    }

    public static async Task<IResult> GetAllByListId(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        ICategoryService categoryService
    )
    {
        var userId = GetUserAndLog(user, $"{nameof(Category)} {nameof(GetAllByListId)} {listId}");
        var categories = await categoryService.GetAllByListIdAsync(userId, listId);
        return Results.Ok(categories);
    }

    public static async Task<IResult> Create(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        [FromBody] CategoryRequest category,
        ICategoryService categoryService,
        IListService listService
    )
    {
        var userId = GetUserAndLog(user, $"{nameof(Create)} {nameof(Category)} in {listId}");
        await ThrowIfListDoesNotExist(listService, userId, listId);
        var createdCategory = await categoryService.CreateAsync(userId, listId, category);
        return Results.Created($"api/category/{createdCategory!.Id}", createdCategory);
    }

    public static async Task<IResult> Update(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        [FromRoute] string id,
        [FromBody] CategoryRequest category,
        ICategoryService categoryService,
        IListService listService
    )
    {
        var userId = GetUserAndLog(user, $"{nameof(Update)} {nameof(Category)} {id} in {listId}");
        await ThrowIfListDoesNotExist(listService, userId, listId);
        var updatedCategory = await categoryService.UpdateAsync(userId, listId, id, category);
        return Results.Ok(updatedCategory);
    }

    public static async Task<IResult> DeleteById(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        [FromRoute] string id,
        ICategoryService categoryService,
        IListService listService,
        IItemService itemService
    )
    {
        var userId = GetUserAndLog(
            user,
            $"{nameof(Category)} {nameof(DeleteById)} {id} in {listId}"
        );
        await ThrowIfListDoesNotExist(listService, userId, listId);
        var defaultCategory = await categoryService.GetDefaultCategory(userId, listId);
        await UpdateItemsToDefaultCategory(itemService, listId, userId, defaultCategory.Id, id);
        await categoryService.DeleteByIdAsync(userId, listId, id);
        return Results.NoContent();
    }

    public static async Task<IResult> ResetByListId(
        ClaimsPrincipal user,
        [FromRoute] string listId,
        ICategoryService categoryService,
        IListService listService,
        IItemService itemService
    )
    {
        var userId = GetUserAndLog(user, $"{nameof(Category)} {nameof(ResetByListId)} {listId}");
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

    private static string GetUserAndLog(ClaimsPrincipal user, string message)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var email =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                ? user.FindFirst(ClaimTypes.Email)?.Value
                : "";

        if (userId is not null)
        {
            Logger.Log($"Request from {email}, id {user}: {message}");
            return userId;
        }

        const string errorMessage = "User is not authenticated or cannot be identified";
        Logger.Log(errorMessage);
        throw new BadRequestException(errorMessage);
    }

    private static async Task ThrowIfListDoesNotExist(
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
