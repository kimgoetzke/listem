using Listem.API.Contracts;
using Listem.API.Domain.Items;
using Listem.API.Domain.Lists;
using Listem.API.Middleware;
using Microsoft.AspNetCore.Mvc;
using static Listem.API.Utilities.EndpointUtilities;

namespace Listem.API.Domain.Categories;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("api/categories", GetAll).RequireAuthorization();
        endpoints.MapGet("api/lists/{listId}/categories", GetAllByListId).RequireAuthorization();
        endpoints.MapPost("api/lists/{listId}/categories", Create).RequireAuthorization();
        endpoints.MapPut("api/lists/{listId}/categories/{id}", Update).RequireAuthorization();
        endpoints
            .MapDelete("api/lists/{listId}/categories/{id}", DeleteById)
            .RequireAuthorization();
        endpoints.MapDelete("api/lists/{listId}/categories", ResetByListId).RequireAuthorization();
    }

    private static async Task<IResult> GetAll(IRequestContext req, ICategoryService categoryService)
    {
        var categories = await categoryService.GetAllAsync(req.UserId);
        return Results.Ok(categories);
    }

    private static async Task<IResult> GetAllByListId(
        IRequestContext req,
        [FromRoute] string listId,
        ICategoryService categoryService
    )
    {
        var categories = await categoryService.GetAllByListIdAsync(req.UserId, listId);
        return Results.Ok(categories);
    }

    private static async Task<IResult> Create(
        IRequestContext req,
        [FromRoute] string listId,
        [FromBody] CategoryRequest category,
        ICategoryService categoryService,
        IListService listService
    )
    {
        await ThrowIfListDoesNotExist(listService, req.UserId, listId);
        var createdCategory = await categoryService.CreateAsync(req.UserId, listId, category);
        return Results.Created(
            $"api/lists/{listId}/categories/{createdCategory!.Id}",
            createdCategory
        );
    }

    private static async Task<IResult> Update(
        IRequestContext req,
        [FromRoute] string listId,
        [FromRoute] string id,
        [FromBody] CategoryRequest category,
        ICategoryService categoryService,
        IListService listService
    )
    {
        await ThrowIfListDoesNotExist(listService, req.UserId, listId);
        var updatedCategory = await categoryService.UpdateAsync(req.UserId, listId, id, category);
        return Results.Ok(updatedCategory);
    }

    private static async Task<IResult> DeleteById(
        IRequestContext req,
        [FromRoute] string listId,
        [FromRoute] string id,
        ICategoryService categoryService,
        IListService listService,
        IItemService itemService
    )
    {
        await ThrowIfListDoesNotExist(listService, req.UserId, listId);
        var defaultCategory = await categoryService.GetDefaultCategory(req.UserId, listId);
        await UpdateItemsToDefaultCategory(itemService, listId, req.UserId, defaultCategory.Id, id);
        await categoryService.DeleteByIdAsync(req.UserId, listId, id);
        return Results.NoContent();
    }

    private static async Task<IResult> ResetByListId(
        IRequestContext req,
        [FromRoute] string listId,
        ICategoryService categoryService,
        IListService listService,
        IItemService itemService
    )
    {
        await ThrowIfListDoesNotExist(listService, req.UserId, listId);
        var defaultCategory = await categoryService.GetDefaultCategory(req.UserId, listId);
        await UpdateItemsToDefaultCategory(itemService, listId, req.UserId, defaultCategory.Id);
        await categoryService.DeleteAllByListIdAsync(req.UserId, listId, defaultCategory.Id);
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
