using Listem.API.Contracts;
using Listem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Listem.API.Controllers;

[Route("api/item-lists")]
[ApiController]
public class ItemListController(ItemListService itemListService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var itemLists = await itemListService.GetAllAsync();
        return Ok(itemLists);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        var itemList = await itemListService.GetByIdAsync(id);
        if (itemList is null)
        {
            return NotFound();
        }

        return Ok(itemList);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemListRequest itemList)
    {
        var createdItemList = await itemListService.CreateAsync(itemList);
        if (createdItemList is null)
        {
            return Conflict();
        }

        return Ok(createdItemList);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] ItemListRequest itemList
    )
    {
        var updatedItemList = await itemListService.UpdateAsync(id, itemList);
        if (updatedItemList is null)
        {
            return NotFound();
        }

        return Ok(updatedItemList);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        return await itemListService.DeleteAsync(id) ? NotFound() : NoContent();
    }
}
