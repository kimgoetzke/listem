using Listem.API.Domain.Categories;
using Listem.API.Domain.Items;
using Listem.API.Domain.Lists;
using Listem.API.Middleware;
using Listem.Contracts;
using Microsoft.AspNetCore.Mvc;
using static Listem.API.Utilities.EndpointUtilities;

namespace Listem.API.Endpoints;

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

    private static async Task<IResult> GetAll(ICategoryService categoryService)
    {
        var categories = await categoryService.GetAllAsync();
        return Results.Ok(categories);
    }

    private static async Task<IResult> GetAllByListId(
        [FromRoute] string listId,
        ICategoryService categoryService
    )
    {
        var categories = await categoryService.GetAllByListIdAsync(listId);
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
        await ThrowIfListDoesNotExist(listService, listId);
        var createdCategory = await categoryService.CreateAsync(req.UserId, listId, category);
        return Results.Created(
            $"api/lists/{listId}/categories/{createdCategory!.Id}",
            createdCategory
        );
    }

    private static async Task<IResult> Update(
        [FromRoute] string listId,
        [FromRoute] string id,
        [FromBody] CategoryRequest category,
        ICategoryService categoryService,
        IListService listService
    )
    {
        await ThrowIfListDoesNotExist(listService, listId);
        var updatedCategory = await categoryService.UpdateAsync(listId, id, category);
        return Results.Ok(updatedCategory);
    }

    private static async Task<IResult> DeleteById(
        [FromRoute] string listId,
        [FromRoute] string id,
        ICategoryService categoryService,
        IListService listService,
        IItemService itemService
    )
    {
        await ThrowIfListDoesNotExist(listService, listId);
        var defaultCategory = await categoryService.GetDefaultCategory(listId);
        await UpdateItemsToDefaultCategory(itemService, listId, defaultCategory.Id, id);
        await categoryService.DeleteByIdAsync(listId, id);
        return Results.NoContent();
    }

    private static async Task<IResult> ResetByListId(
        [FromRoute] string listId,
        ICategoryService categoryService,
        IListService listService,
        IItemService itemService
    )
    {
        await ThrowIfListDoesNotExist(listService, listId);
        var defaultCategory = await categoryService.GetDefaultCategory(listId);
        await UpdateItemsToDefaultCategory(itemService, listId, defaultCategory.Id);
        await categoryService.DeleteAllByListIdAsync(listId, defaultCategory.Id);
        return Results.NoContent();
    }

    private static async Task UpdateItemsToDefaultCategory(
        IItemService itemService,
        string listId,
        string defaultCategoryId,
        string? currentCategoryId = null
    )
    {
        if (currentCategoryId is null)
        {
            await itemService.UpdateToDefaultCategoryAsync(listId, defaultCategoryId);
            return;
        }
        await itemService.UpdateToDefaultCategoryAsync(
            listId,
            defaultCategoryId,
            currentCategoryId
        );
    }
}
