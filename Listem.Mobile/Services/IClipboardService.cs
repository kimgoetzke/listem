using System.Collections.ObjectModel;
using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public interface IClipboardService
{
  Task InsertFromClipboardAsync(
    ObservableCollection<ObservableItem> items,
    ObservableCollection<ObservableCategory> categories,
    string listId
  );

  void CopyToClipboard(
    ObservableCollection<ObservableItem> items,
    ObservableCollection<ObservableCategory> categories
  );
}
