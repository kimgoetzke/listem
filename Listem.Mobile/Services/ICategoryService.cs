using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public interface ICategoryService
{
  Task<List<ObservableCategory>> GetAllByListIdAsync(string listId);
  Task CreateOrUpdateAsync(ObservableCategory observableCategory);
  Task DeleteAsync(ObservableCategory observableCategory);
  Task DeleteAllByListIdAsync(string listId);
}
