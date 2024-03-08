using Listem.API.Contracts;
using Listem.API.Domain.Lists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Listem.API.Domain.Items;

[Route("api/items")]
[ApiController]
public class ItemController(IItemService itemService, IListService listService)
    : ApiController(listService)
{
    [HttpGet, Authorize]
    public async Task<IActionResult> GetAll()
    {
        var userId = ValidateUserRequestOrThrow("GET all");
        var items = await itemService.GetAllAsync(userId);
        return Ok(items);
    }

    [HttpGet("{listId}"), Authorize]
    public async Task<IActionResult> GetAllByListId([FromRoute] string listId)
    {
        var userId = ValidateUserRequestOrThrow($"GET all for list {listId}");
        await ThrowIfListDoesNotExist(userId, listId);
        var items = await itemService.GetAllByListIdAsync(userId, listId);
        return Ok(items);
    }

    [HttpPost("{listId}"), Authorize]
    public async Task<IActionResult> Create([FromRoute] string listId, [FromBody] ItemRequest item)
    {
        var userId = ValidateUserRequestOrThrow($"POST {item} to list {listId}");
        await ThrowIfListDoesNotExist(userId, listId);
        var createdItem = await itemService.CreateAsync(userId, listId, item);
        return Created($"api/items/{createdItem!.Id}", createdItem);
    }

    [HttpPut("{listId}/{id}"), Authorize]
    public async Task<IActionResult> Update(
        [FromRoute] string listId,
        [FromRoute] string id,
        [FromBody] ItemRequest item
    )
    {
        var userId = ValidateUserRequestOrThrow($"UPDATE {item} in list {listId}");
        var updatedItem = await itemService.UpdateAsync(userId, listId, id, item);
        return Ok(updatedItem);
    }

    [HttpDelete("{listId}/{id}"), Authorize]
    public async Task<IActionResult> Delete([FromRoute] string listId, [FromRoute] string id)
    {
        var userId = ValidateUserRequestOrThrow($"DELETE {id} in list {listId}");
        await ThrowIfListDoesNotExist(userId, listId);
        await itemService.DeleteByIdAsync(userId, listId, id);
        return NoContent();
    }

    [HttpDelete("{listId}"), Authorize]
    public async Task<IActionResult> DeleteAllByList([FromRoute] string listId)
    {
        var userId = ValidateUserRequestOrThrow($"DELETE all for list {listId}");
        await ThrowIfListDoesNotExist(userId, listId);
        await itemService.DeleteAllByListIdAsync(userId, listId);
        return NoContent();
    }
}
