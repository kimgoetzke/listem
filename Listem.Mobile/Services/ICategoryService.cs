using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public interface ICategoryService
{
  Task CreateAsync(Category category, List list);
  Task CreateAllAsync(IList<Category> categories, List list);
  Task DeleteAsync(Category category);
  Task ResetAsync(List list);
}
