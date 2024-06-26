﻿using Listem.Mobile.Models;
using Microsoft.Extensions.Logging;

namespace Listem.Mobile.Services;

public class ListService(ILogger<CategoryService> logger) : IListService
{
  public async Task CreateAsync(List list)
  {
    var realm = RealmService.GetMainThreadRealm();
    await realm.WriteAsync(() =>
    {
      list.IsDraft = false;
      list.Categories.Add(new Category { Name = Constants.DefaultCategoryName });
      realm.Add(list);
      logger.Info("Added: {List}", list.ToLog());
    });
  }

  public async Task UpdateAsync(
    List list,
    string? name = null,
    string? ownedBy = null,
    ISet<string>? sharedWith = null,
    string? listType = null
  )
  {
    var realm = RealmService.GetMainThreadRealm();
    if (realm.Find<List>(list.Id) == null)
    {
      logger.Info("Not updated because it doesn't exist: {List}", list.ToLog());
      return;
    }
    await realm.WriteAsync(() =>
    {
      list.Name = name ?? list.Name;
      list.OwnedBy = ownedBy ?? list.OwnedBy;
      if (sharedWith != null)
      {
        list.SharedWith.Clear();
        foreach (var id in sharedWith)
        {
          list.SharedWith.Add(id);
        }
      }
      list.ListType = listType ?? list.ListType;
      list.UpdatedOn = DateTimeOffset.Now;

      logger.Info("Updated: {List}", list.ToLog());
    });
  }

  public async Task MarkAsUpdatedAsync(List list)
  {
    var realm = RealmService.GetMainThreadRealm();
    await realm.WriteAsync(() =>
    {
      list.UpdatedOn = DateTimeOffset.Now;
      logger.Info("Marked as updated: {List}", list.ToLog());
    });
  }

  public async Task DeleteAsync(List list)
  {
    logger.Info("Removing: {List}", list.ToLog());
    var realm = RealmService.GetMainThreadRealm();
    await realm.WriteAsync(() => realm.Remove(list));
  }

  public async Task<bool> ShareWith(List list, string email)
  {
    if (await RealmService.ResolveToUserId(email) is not { } id)
    {
      logger.Info("Cannot share list with '{User}' - user not found", email);
      return false;
    }
    var realm = RealmService.GetMainThreadRealm();
    await realm.WriteAsync(() =>
    {
      list.SharedWith.Add(id);
      list.UpdatedOn = DateTimeOffset.Now;
      foreach (var item in list.Items)
      {
        item.SharedWith.Add(id);
        item.UpdatedOn = DateTimeOffset.Now;
        logger.Info("Shared: '{Item}' with {User}", item.ToLog(), id);
      }
    });
    logger.Info("Shared: '{List}' with {User}", list.ToLog(), id);
    return true;
  }

  public async Task<bool> RevokeAccess(List list, string id)
  {
    // Handled using serverless function because caller will, by definition, lack permission
    // to write to a document if they are no longer shared with it.
    var result = await RealmService.RevokeAccess(list.Id.ToString(), id);
    logger.Info(
      "{Result} access of user {User} from {List}",
      result ? "Removed" : "Failed to remove",
      id,
      list.ToLog()
    );
    return result;
  }
}
