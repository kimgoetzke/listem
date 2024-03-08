using Listem.API.Contracts;
using Listem.API.Domain.Items;
using Listem.API.Domain.Lists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Listem.API.Domain.Categories;

[Route("api/categories")]
public class CategoryController(
    ICategoryService categoryService,
    IListService listService,
    IItemService itemService
) : ApiController(listService)
{
    [HttpGet, Authorize]
    public async Task<IActionResult> GetAll()
    {
        var userId = ValidateUserRequestOrThrow("GET all");
        var categories = await categoryService.GetAllAsync(userId);
        return Ok(categories);
    }

    [HttpGet("{listId}"), Authorize]
    public async Task<IActionResult> GetAllByListId([FromRoute] string listId)
    {
        var userId = ValidateUserRequestOrThrow($"GET all for list {listId}");
        var categories = await categoryService.GetAllByListIdAsync(userId, listId);
        return Ok(categories);
    }

    [HttpPost("{listId}"), Authorize]
    public async Task<IActionResult> Create(
        [FromRoute] string listId,
        [FromBody] CategoryRequest category
    )
    {
        var userId = ValidateUserRequestOrThrow($"POST {category} to list {listId}");
        await ThrowIfListDoesNotExist(userId, listId);
        var createdCategory = await categoryService.CreateAsync(userId, listId, category);
        return Created($"api/category/{createdCategory!.Id}", createdCategory);
    }

    [HttpPut("{listId}/{id}"), Authorize]
    public async Task<IActionResult> Update(
        [FromRoute] string listId,
        [FromRoute] string id,
        [FromBody] CategoryRequest category
    )
    {
        var userId = ValidateUserRequestOrThrow($"UPDATE {category} in list {listId}");
        await ThrowIfListDoesNotExist(userId, listId);
        var updatedCategory = await categoryService.UpdateAsync(userId, listId, id, category);
        return Ok(updatedCategory);
    }

    [HttpDelete("{listId}/{id}"), Authorize]
    public async Task<IActionResult> DeleteById([FromRoute] string listId, [FromRoute] string id)
    {
        var userId = ValidateUserRequestOrThrow($"DELETE {id} in list {listId}");
        await ThrowIfListDoesNotExist(userId, listId);
        var defaultCategory = await categoryService.GetDefaultCategory(userId, listId);
        await UpdateItemsToDefaultCategory(listId, userId, defaultCategory.Id, id);
        await categoryService.DeleteByIdAsync(userId, listId, id);
        return NoContent();
    }

    [HttpDelete("{listId}"), Authorize]
    public async Task<IActionResult> ResetByListId([FromRoute] string listId)
    {
        var userId = ValidateUserRequestOrThrow($"DELETE all (except default) for list {listId}");
        await ThrowIfListDoesNotExist(userId, listId);
        var defaultCategory = await categoryService.GetDefaultCategory(userId, listId);
        await UpdateItemsToDefaultCategory(listId, userId, defaultCategory.Id);
        await categoryService.DeleteAllByListIdAsync(userId, listId, defaultCategory.Id);
        return NoContent();
    }

    private async Task UpdateItemsToDefaultCategory(
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
