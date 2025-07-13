using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public interface IListService
{
  Task<List<ObservableList>> GetAllAsync();
  Task CreateOrUpdateAsync(ObservableList observableList);
  Task MarkAsUpdatedAsync(ObservableList observableList);
  Task DeleteAsync(ObservableList observableList);
  Task DeleteAllAsync();
}
