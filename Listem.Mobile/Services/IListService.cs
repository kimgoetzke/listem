using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public interface IListService
{
    Task CreateOrUpdateAsync(List list);
    Task DeleteAsync(List list);
}
