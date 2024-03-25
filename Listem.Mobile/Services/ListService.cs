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
            logger.LogInformation("Added: {List}", list.ToLoggableString());
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
            logger.LogInformation(
                "Not updated because it doesn't exist: {List}",
                list.ToLoggableString()
            );
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

            logger.LogInformation("Updated: {List}", list.ToLoggableString());
        });
    }

    public async Task DeleteAsync(List list)
    {
        logger.LogInformation("Removing: {List}", list.ToLoggableString());
        await _realm.WriteAsync(() => _realm.Remove(list));
    }
}
