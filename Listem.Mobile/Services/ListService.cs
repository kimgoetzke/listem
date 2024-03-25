using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Realms;

namespace Listem.Mobile.Services;

public class ListService : IListService
{
    private readonly Realm _realm = RealmService.GetMainThreadRealm();

    public async Task CreateOrUpdateAsync(List list)
    {
        await _realm.WriteAsync(() =>
        {
            if (list.IsDraft)
            {
                var defaultCategory = new Category { Name = Constants.DefaultCategoryName };
                list.Categories.Add(defaultCategory);
                list.IsDraft = false;
                _realm.Add(list);
                Logger.Log($"Added list: {list.ToLoggableString()}");
            }
            else
            {
                var existingList = _realm.Find<List>(list.Id);
                if (existingList == null)
                {
                    Logger.Log($"List has an id but couldn't be found: {list.ToLoggableString()}");
                    return;
                }
                existingList.Name = list.Name;
                existingList.ListType = list.ListType;
                existingList.UpdatedOn = list.UpdatedOn;
                Logger.Log($"Updated list: {list.ToLoggableString()}");
            }
        });
    }

    public async Task DeleteAsync(List list)
    {
        Logger.Log($"Removing list: '{list.Name}' {list.Id}");
        await _realm.WriteAsync(() => _realm.Remove(list));
    }
}
