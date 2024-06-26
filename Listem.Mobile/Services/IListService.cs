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
  Task MarkAsUpdatedAsync(List list);
  Task DeleteAsync(List list);
  Task<bool> ShareWith(List list, string email);
  Task<bool> RevokeAccess(List list, string id);
}
