﻿using Microsoft.EntityFrameworkCore;

namespace Listem.API.Domain.Items;

#pragma warning disable CS1998
internal class ItemRepository(ItemDbContext dbContext, ILogger<ItemRepository> logger)
    : IItemRepository
{
    public Task<List<Item>> GetAllAsync(string userId)
    {
        var items = dbContext.Items.Where(i => i.OwnerId == userId).ToList();
        logger.LogInformation("Retrieved {Count} items", items.Count);
        return Task.FromResult(items);
    }

    public Task<List<Item>> GetAllByListIdAsync(string userId, string listId)
    {
        var items = dbContext.Items.Where(i => i.ListId == listId && i.OwnerId == userId).ToList();
        logger.LogInformation("Retrieved {Count} items for list {ListId}", items.Count, listId);
        return Task.FromResult(items);
    }

    public Task<Item?> GetByIdAsync(string userId, string itemId)
    {
        var item = dbContext.Items.FirstOrDefault(i => i.Id == itemId && i.OwnerId == userId);
        logger.LogInformation("Retrieved item: {Item}", item?.ToString() ?? "null");
        return Task.FromResult(item);
    }

    public async Task<Item?> CreateAsync(Item item)
    {
        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Added item: {Item}", item);
        return dbContext.Items.FirstOrDefault(i => i.Id == item.Id);
    }

    public async Task<Item?> UpdateAsync(Item item)
    {
        var existingItem = dbContext.Items.FirstOrDefault(i => i.Id == item.Id);

        if (existingItem is null)
            return null;

        existingItem.Name = item.Name;
        existingItem.IsImportant = item.IsImportant;
        existingItem.CategoryId = item.CategoryId;
        existingItem.Quantity = item.Quantity;
        existingItem.UpdatedOn = DateTime.Now;
        logger.LogInformation("Updated item: {Item}", existingItem);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("The item was updated by another process");
        }
        return existingItem;
    }

    public async Task<bool> DeleteAllByListIdAsync(string userId, string listId)
    {
        var toDelete = dbContext.Items.Where(i => i.ListId == listId && i.OwnerId == userId);
        logger.LogInformation(
            "Removing all {Count} items in list {ListId} by {UserId}",
            toDelete.Count(),
            listId,
            userId
        );
        dbContext.Items.RemoveRange(toDelete);
        return await dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteByIdAsync(string userId, string listId, string itemId)
    {
        logger.LogInformation("Removing item: {ItemId} by {UserId}", itemId, userId);
        var toDelete = dbContext.Items.Where(i =>
            i.Id == itemId && i.ListId == listId && i.OwnerId == userId
        );
        dbContext.Items.RemoveRange(toDelete);
        return await dbContext.SaveChangesAsync() > 0;
    }
#pragma warning restore CS1998
}
