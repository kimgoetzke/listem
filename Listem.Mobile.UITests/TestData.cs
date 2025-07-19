namespace Listem.Mobile.UITests;

public abstract class TestData
{
  private static List<TestItem> StoreListItems { get; } =
    [
      new("Item-0", AutomationIdModel.DefaultCategoryName, 1, false),
      new("Item-1", "Store-1", 1, false),
      new("Item-2", "Store-1", 1, true),
      new("Item-3", "Store-2", 2, true),
      new("Item-4", "Store-2", 5, true),
      new("Item-5", AutomationIdModel.DefaultCategoryName, 1, false)
    ];

  private static List<TestItem> StandardListItems { get; } =
    [
      new("Item-0", AutomationIdModel.DefaultCategoryName, 1, false),
      new("Item-1", "Cat-A", 1, false),
      new("Item-2", "Cat-B", 1, true),
      new("Item-3", "Cat-B", 2, true)
    ];

  public static readonly TestList StandardList =
    new("Standard", StandardListItems, AutomationIdModel.DefaultListType, ["Cat-A", "Cat-B"]);

  public static readonly TestList FeatureList =
    new("Features", StoreListItems, AutomationIdModel.ShoppingListType, ["Store-1", "Store-2"]);

  public static readonly TestList ClipboardList =
    new("Clipboard", StoreListItems, AutomationIdModel.ShoppingListType, ["Store-1", "Store-2"]);

  public record TestList(
    string Name,
    List<TestItem> Items,
    string ListType,
    List<string> Categories
  );

  public record TestItem(string Name, string Category, int Quantity, bool IsImportant);
}
