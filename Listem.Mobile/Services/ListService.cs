using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Realms;

namespace Listem.Mobile.Services;

public class ListService : IListService
{
    private readonly Realm _realm = RealmService.GetMainThreadRealm();

    public async Task CreateAsync(List list)
    {
        await _realm.WriteAsync(() =>
        {
            list.IsDraft = false;
            list.Categories.Add(new Category { Name = Constants.DefaultCategoryName });
            _realm.Add(list);
            Logger.Log($"Added: {list.ToLoggableString()}");
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
        await _realm.WriteAsync(() =>
        {
            if (_realm.Find<List>(list.Id) == null)
            {
                Logger.Log($"List has an id but couldn't be found: {list.ToLoggableString()}");
                return;
            }
            if (name != null)
            {
                list.Name = name;
            }
            if (ownedBy != null)
            {
                list.OwnedBy = ownedBy;
            }
            if (sharedWith != null)
            {
                list.SharedWith.Clear();
                foreach (var s in sharedWith)
                {
                    list.SharedWith.Add(s);
                }
            }
            if (listType != null)
            {
                list.ListType = listType;
            }
            list.UpdatedOn = DateTimeOffset.Now.ToUniversalTime();
            Logger.Log($"Updated: {list.ToLoggableString()}");
        });
    }

    public async Task DeleteAsync(List list)
    {
        Logger.Log($"Removing list: '{list.Name}' {list.Id}");
        await _realm.WriteAsync(() => _realm.Remove(list));
    }
}
