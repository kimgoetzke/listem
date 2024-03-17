using Listem.API.Domain.Lists;
using Listem.API.Exceptions;

namespace Listem.API.Utilities;

public static class EndpointUtilities
{
    public static async Task ThrowIfListDoesNotExist(IListService listService, string listId)
    {
        if (!await listService.ExistsAsync(listId))
        {
            throw new BadRequestException($"List {listId} does not exist");
        }
    }
}
