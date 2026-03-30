using Listem.Mobile.Models;

namespace Listem.Mobile.Utilities;

public static class ItemSorter
{
  public static IEnumerable<ObservableItem> FilterForPreview(
    IEnumerable<ObservableItem> items,
    bool isRecurring
  )
  {
    return isRecurring ? items.Where(item => item.IsActive) : items;
  }

  public static IOrderedEnumerable<ObservableItem> Sort(
    IEnumerable<ObservableItem> items,
    bool isRecurring
  )
  {
    return isRecurring
      ? items
        .OrderBy(item => item.IsActive ? 0 : 1)
        .ThenBy(item => item.CategoryName)
        .ThenByDescending(item => item.AddedOn)
      : items.OrderBy(item => item.CategoryName).ThenByDescending(item => item.AddedOn);
  }
}
