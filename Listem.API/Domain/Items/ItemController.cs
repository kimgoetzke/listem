using Listem.API.Domain.Categories;
using Microsoft.AspNetCore.Mvc;

namespace Listem.API.Domain.Items;

[Route("api/item")]
[ApiController]
public class ItemController(IItemService itemService, ICategoryService categoryService)
    : ControllerBase
{
    // TODO: Add controller methods
}