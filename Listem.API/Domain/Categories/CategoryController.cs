using System.Security.Claims;
using Listem.API.Domain.ItemLists;
using Listem.API.Exceptions;
using Listem.API.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Listem.API.Domain.Categories;

[Route("api/category")]
[ApiController]
public class CategoryController(ICategoryService categoryService, IListService listService)
    : ControllerBase
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
        var updatedCategory = await categoryService.UpdateAsync(userId, listId, id, category);
        return Ok(updatedCategory);
    }

    [HttpDelete("{listId}/{id}"), Authorize]
    public async Task<IActionResult> Delete([FromRoute] string listId, [FromRoute] string id)
    {
        var userId = ValidateUserRequestOrThrow($"DELETE {id} in list {listId}");
        await ThrowIfListDoesNotExist(userId, listId);
        await categoryService.DeleteByIdAsync(userId, listId, id);
        return NoContent();
    }

    [HttpDelete("{listId}"), Authorize]
    public async Task<IActionResult> ResetForList([FromRoute] string listId)
    {
        var userId = ValidateUserRequestOrThrow($"DELETE all for list {listId}");
        await ThrowIfListDoesNotExist(userId, listId);
        await categoryService.DeleteAllByListIdAsync(userId, listId);
        return NoContent();
    }

    private string ValidateUserRequestOrThrow(string message)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                ? User.FindFirst(ClaimTypes.Email)?.Value
                : userId;

        if (userId is not null)
        {
            Logger.Log($"Request from {user}: {message}");
            return userId;
        }

        const string errorMessage = "User is not authenticated or cannot be identified";
        Logger.Log(errorMessage);
        throw new BadRequestException(errorMessage);
    }

    private async Task ThrowIfListDoesNotExist(string userId, string listId)
    {
        if (!await listService.ExistsAsync(userId, listId))
        {
            throw new NotFoundException($"List {listId} does not exist");
        }
    }
}
