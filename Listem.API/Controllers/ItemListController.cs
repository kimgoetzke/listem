using System.Net;
using System.Security.Claims;
using Listem.API.Contracts;
using Listem.API.Exceptions;
using Listem.API.Services;
using Listem.API.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Listem.API.Controllers;

[Route("api/item-list")]
[ApiController]
public class ItemListController(ItemListService itemListService) : ControllerBase
{
    [HttpGet, Authorize]
    public async Task<IActionResult> GetAll()
    {
        var userId = ValidateUserRequestOrThrow("GET all");
        var itemLists = await itemListService.GetAllAsync(userId);
        return Ok(itemLists);
    }

    [HttpGet("{id}"), Authorize]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        var userId = ValidateUserRequestOrThrow($"GET {id}");
        var itemList = await itemListService.GetByIdAsync(userId, id);
        return Ok(itemList);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] ItemListRequest itemList)
    {
        var userId = ValidateUserRequestOrThrow($"POST {itemList}");
        var createdItemList = await itemListService.CreateAsync(userId, itemList);
        return Created($"api/item-list/{createdItemList!.Id}", createdItemList);
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] ItemListRequest itemList
    )
    {
        var userId = ValidateUserRequestOrThrow($"UPDATE {itemList}");
        var updatedItemList = await itemListService.UpdateAsync(userId, id, itemList);
        return Ok(updatedItemList);
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var userId = ValidateUserRequestOrThrow($"DELETE {id}");
        await itemListService.DeleteByIdAsync(userId, id);
        return NoContent();
    }

    private string ValidateUserRequestOrThrow(string message)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        if (userId is not null)
        {
            Logger.Log($"Request from {email}: {message}");
            return userId;
        }

        const string errorMessage = "User is not authenticated or cannot be identified";
        Logger.Log(errorMessage);
        throw new BadRequestException(errorMessage);
    }
}
