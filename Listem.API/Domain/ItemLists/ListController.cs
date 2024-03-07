using System.Security.Claims;
using Listem.API.Domain.Categories;
using Listem.API.Exceptions;
using Listem.API.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Listem.API.Domain.ItemLists;

[Route("api/list")]
[ApiController]
public class ListController(IListService listService, ICategoryService categoryService)
    : ControllerBase
{
    [HttpGet, Authorize]
    public async Task<IActionResult> GetAll()
    {
        var userId = ValidateUserRequestOrThrow("GET all");
        var itemLists = await listService.GetAllAsync(userId);
        return Ok(itemLists);
    }

    [HttpGet("{id}"), Authorize]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        var userId = ValidateUserRequestOrThrow($"GET {id}");
        var itemList = await listService.GetByIdAsync(userId, id);
        return Ok(itemList);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] ListRequest list)
    {
        var userId = ValidateUserRequestOrThrow($"POST {list}");
        var createdItemList = await listService.CreateAsync(userId, list);
        var defaultCategory = new CategoryRequest { Name = ICategoryService.DefaultCategoryName };
        await categoryService.CreateAsync(userId, createdItemList!.Id, defaultCategory);
        return Created($"api/list/{createdItemList.Id}", createdItemList);
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] ListRequest list
    )
    {
        var userId = ValidateUserRequestOrThrow($"UPDATE {list}");
        var updatedItemList = await listService.UpdateAsync(userId, id, list);
        return Ok(updatedItemList);
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var userId = ValidateUserRequestOrThrow($"DELETE {id}");
        await categoryService.DeleteAllByListIdAsync(userId, id);
        await listService.DeleteByIdAsync(userId, id);
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
}
