using Listem.API.Contracts;
using Listem.API.Domain.Categories;
using Listem.API.Domain.Items;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Listem.API.Domain.Lists;

[Route("api/lists")]
[ApiController]
public class ListController(
    IListService listService,
    ICategoryService categoryService,
    IItemService itemService
) : ApiController(listService)
{
    private readonly IListService _listService = listService;

    [HttpGet, Authorize]
    public async Task<IActionResult> GetAll()
    {
        var userId = ValidateUserRequestOrThrow("GET all");
        var itemLists = await _listService.GetAllAsync(userId);
        return Ok(itemLists);
    }

    [HttpGet("{id}"), Authorize]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        var userId = ValidateUserRequestOrThrow($"GET {id}");
        var itemList = await _listService.GetByIdAsync(userId, id);
        return Ok(itemList);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] ListRequest list)
    {
        var userId = ValidateUserRequestOrThrow($"POST {list}");
        var createdItemList = await _listService.CreateAsync(userId, list);
        var defaultCategory = new CategoryRequest { Name = ICategoryService.DefaultCategoryName };
        await categoryService.CreateAsync(userId, createdItemList!.Id, defaultCategory);
        return Created($"api/list/{createdItemList.Id}", createdItemList);
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] ListRequest list)
    {
        var userId = ValidateUserRequestOrThrow($"UPDATE {list}");
        var updatedItemList = await _listService.UpdateAsync(userId, id, list);
        return Ok(updatedItemList);
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var userId = ValidateUserRequestOrThrow($"DELETE {id}");
        await ThrowIfListDoesNotExist(userId, id);
        await categoryService.DeleteAllByListIdAsync(userId, id);
        await itemService.DeleteAllByListIdAsync(userId, id);
        await _listService.DeleteByIdAsync(userId, id);
        return NoContent();
    }
}
