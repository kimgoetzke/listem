using Listem.API.Contracts;
using Listem.API.Exceptions;

namespace Listem.API.Domain.Lists;

internal class ListService(IListRepository listRepository) : IListService
{
    public async Task<List<ListResponse>> GetAllAsync(string userId)
    {
        var result = await listRepository.GetAllAsync(userId);
        return result.Select(l => l.ToResponse()).ToList();
    }

    public async Task<ListResponse?> GetByIdAsync(string userId, string listId)
    {
        var result = await listRepository.GetByIdAsync(userId, listId);
        return result is not null
            ? result.ToResponse()
            : throw new NotFoundException($"List {listId} not found");
    }

    public Task<bool> ExistsAsync(string userId, string listId)
    {
        return listRepository.ExistsAsync(userId, listId);
    }

    public async Task<ListResponse?> CreateAsync(string userId, ListRequest listRequest)
    {
        var toCreate = List.From(listRequest, userId);
        var result = await listRepository.CreateAsync(toCreate);

        if (result is null)
        {
            throw new ConflictException("List cannot be created, it already exists");
        }

        return result.ToResponse();
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

        existing.Update(requested);
        var result = await listRepository.UpdateAsync(existing);

        if (result is not null)
            return result.ToResponse();

        throw new SystemException($"Failed to update list {listId} even though it was found");
    }

    public async Task DeleteByIdAsync(string userId, string listId)
    {
        var hasBeenDeleted = await listRepository.DeleteByIdAsync(userId, listId);
        if (!hasBeenDeleted)
        {
            throw new NotFoundException(
                $"Failed to delete list {listId} because it doesn't exist or user has no access to it"
            );
        }
    }
}
