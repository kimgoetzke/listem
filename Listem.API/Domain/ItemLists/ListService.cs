using Listem.API.Exceptions;

namespace Listem.API.Domain.ItemLists;

public class ListService(IListRepository listRepository) : IListService
{
    public async Task<List<ListResponse>> GetAllAsync(string userId)
    {
        var itemLists = await listRepository.GetAllAsync(userId);
        return itemLists.Select(ListResponse.FromItemList).ToList();
    }

    public async Task<ListResponse?> GetByIdAsync(string userId, string listId)
    {
        var fetched = await listRepository.GetByIdAsync(userId, listId);
        return fetched is not null
            ? ListResponse.FromItemList(fetched)
            : throw new NotFoundException("List not found");
    }

    public Task<bool> ExistsAsync(string userId, string listId)
    {
        return listRepository.ExistsAsync(userId, listId);
    }

    public async Task<ListResponse?> CreateAsync(string userId, ListRequest listRequest)
    {
        var toCreate = listRequest.ToItemList(userId);
        var result = await listRepository.CreateAsync(toCreate);
        // await CreateDefaultCategory(itemList.Id);
        return result is not null
            ? ListResponse.FromItemList(result)
            : throw new ConflictException("List cannot be created, it already exists");
    }

    public async Task<ListResponse?> UpdateAsync(
        string userId,
        string listId,
        ListRequest requested
    )
    {
        var existing = await listRepository.GetByIdAsync(userId, listId);

        if (existing is null)
            throw new NotFoundException(
                $"Failed to update list {listId} because it does not exist"
            );

        var toUpdate = requested.ToItemList(existing);
        var result = await listRepository.UpdateAsync(toUpdate);

        if (result is not null)
            return ListResponse.FromItemList(result);

        throw new NotFoundException($"Failed to update list {listId} even though it was found");
    }

    public async Task DeleteByIdAsync(string userId, string listId)
    {
        var hasBeenDeleted = await listRepository.DeleteByIdAsync(userId, listId);
        if (!hasBeenDeleted)
        {
            throw new NotFoundException($"Failed to delete list {listId}");
        }
    }
}
