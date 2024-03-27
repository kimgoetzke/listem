using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public interface IListService
{
  Task CreateAsync(List list);
  Task UpdateAsync(
    List list,
    string? name = null,
    string? ownedBy = null,
    ISet<string>? sharedWith = null,
    string? listType = null
  );
  Task DeleteAsync(List list);
  Task ShareWith(List list, string userName);
  Task RevokeAccess(List list, string userName);
}
