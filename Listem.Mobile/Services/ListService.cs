using Listem.Mobile.Models;
using Listem.Mobile.Utilities;

namespace Listem.Mobile.Services;

public class ListService : ILocalListService
{
    public async Task<List<ObservableList>> GetAllAsync()
    {
        Logger.Log("Getting all lists");
        return [];
    }

    public async Task CreateOrUpdateAsync(ObservableList observableList)
    {
        Logger.Log($"Added list: {observableList.ToLoggableString()}");
    }

    public async Task DeleteAsync(ObservableList observableList)
    {
        Logger.Log($"Removing list: '{observableList.Name}' {observableList.Id}");
    }

    public async Task DeleteAllAsync()
    {
        Logger.Log($"Removed all lists");
    }
}
