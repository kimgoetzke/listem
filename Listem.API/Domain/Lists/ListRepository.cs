using Listem.API.Middleware;
using Microsoft.EntityFrameworkCore;

namespace Listem.API.Domain.Lists;

#pragma warning disable CS1998
internal class ListRepository(
    ListDbContext dbContext,
    ILogger<ListRepository> logger,
    IRequestContext reqContext
) : IListRepository
{
    public Task<List<List>> GetAllAsync()
    {
        var lists = dbContext.Lists.Where(i => i.OwnerId == reqContext.UserId).ToList();
        logger.LogInformation("Retrieved {Count} lists", lists.Count);
        return Task.FromResult(lists);
    }

    public Task<List?> GetByIdAsync(string listId)
    {
        var list = dbContext.Lists.FirstOrDefault(i =>
            i.Id == listId && i.OwnerId == reqContext.UserId
        );
        logger.LogInformation("Retrieved list: {Item}", list?.ToString() ?? "null");
        return Task.FromResult(list);
    }

    public Task<bool> ExistsAsync(string listId)
    {
        return Task.FromResult(
            dbContext.Lists.Any(i => i.Id == listId && i.OwnerId == reqContext.UserId)
        );
    }

    public async Task<List?> CreateAsync(List list)
    {
        dbContext.Lists.Add(list);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Added list: {List}", list);
        return dbContext.Lists.FirstOrDefault(i => i.Id == list.Id);
    }

    public async Task<List?> UpdateAsync(List list)
    {
        var existingList = dbContext.Lists.FirstOrDefault(i => i.Id == list.Id);

        if (existingList is null)
            return null;

        existingList.Name = list.Name;
        existingList.ListType = list.ListType;
        existingList.UpdatedOn = DateTime.Now;
        logger.LogInformation("Updated list: {List}", existingList);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("The list was updated by another process");
        }
        return existingList;
    }

    public async Task<bool> DeleteByIdAsync(string listId)
    {
        logger.LogInformation("Removing list: {ListId} by {UserId}", listId, reqContext.UserId);
        var toDelete = dbContext.Lists.Where(i => i.Id == listId && i.OwnerId == reqContext.UserId);
        dbContext.Lists.RemoveRange(toDelete);
        return await dbContext.SaveChangesAsync() > 0;
    }
#pragma warning restore CS1998
}
