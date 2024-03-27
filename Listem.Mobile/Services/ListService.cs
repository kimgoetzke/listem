using Listem.Mobile.Models;
using Microsoft.Extensions.Logging;
using Realms;

namespace Listem.Mobile.Services;

public class ListService(ILogger<CategoryService> logger) : IListService
{
  private readonly Realm _realm = RealmService.GetMainThreadRealm();

  public async Task CreateAsync(List list)
  {
    await _realm.WriteAsync(() =>
    {
      list.IsDraft = false;
      list.Categories.Add(new Category { Name = Constants.DefaultCategoryName });
      _realm.Add(list);
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
    if (_realm.Find<List>(list.Id) == null)
    {
      logger.Info("Not updated because it doesn't exist: {List}", list.ToLog());
      return;
    }
    await _realm.WriteAsync(() =>
    {
      list.Name = name ?? list.Name;
      list.OwnedBy = ownedBy ?? list.OwnedBy;
      if (sharedWith != null)
      {
        list.SharedWith.Clear();
        foreach (var s in sharedWith)
        {
          list.SharedWith.Add(s);
        }
      }
      list.ListType = listType ?? list.ListType;
      list.UpdatedOn = DateTimeOffset.Now.ToUniversalTime();

      logger.Info("Updated: {List}", list.ToLog());
    });
  }

  public async Task DeleteAsync(List list)
  {
    logger.Info("Removing: {List}", list.ToLog());
    await _realm.WriteAsync(() => _realm.Remove(list));
  }

  public async Task ShareWith(List list, string userName)
  {
    await _realm.WriteAsync(() =>
    {
      list.SharedWith.Add(userName);
      foreach (var item in list.Items)
      {
        item.SharedWith.Add(userName);
      }
    });
    logger.Info("Shared: {List} with {User}", list.Name, userName);
  }

  public async Task RevokeAccess(List list, string userName)
  {
    await _realm.WriteAsync(() =>
    {
      list.SharedWith.Remove(userName);
      foreach (var item in list.Items)
      {
        item.SharedWith.Remove(userName);
      }
    });
    logger.Info("Removed access of user {User} from {List}", userName, list.Name);
  }
}
