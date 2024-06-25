using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public interface IClipboardService
{
  Task InsertFromClipboardAsync(IList<Category> categories, List list);

  void CopyToClipboard(IList<Item> items, IList<Category> categories);
}
