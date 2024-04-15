namespace Listem.Mobile.UITests;

public abstract class TestData
{
  private static List<TestItem> StoreListItems { get; } =
    [
      new TestItem("Item-0", AutomationIdModel.DefaultCategoryName, 1, false),
      new TestItem("Item-1", "Store-1", 1, false),
      new TestItem("Item-2", "Store-1", 1, true),
      new TestItem("Item-3", "Store-2", 2, true),
      new TestItem("Item-4", "Store-2", 5, true),
      new TestItem("Item-5", AutomationIdModel.DefaultCategoryName, 1, false)
    ];

  private static List<TestItem> BasicListItems { get; } =
    [
      new TestItem("OwnerItem", AutomationIdModel.DefaultCategoryName, 1, false),
      new TestItem("CollaboratorItem", AutomationIdModel.DefaultCategoryName, 1, false)
    ];

  private static List<TestUser> Users { get; } =
    [
      new TestUser("someone@example", "Password1!", "660666262693a806a425b3c6"),
      new TestUser("function1@example", "Password1!", "6605ba964a5285f9ee1af6eb"),
      new TestUser("function2@example", "Password1!", "unknown")
    ];

  public static readonly TestList FeatureList =
    new(
      "Features",
      StoreListItems,
      AutomationIdModel.ShoppingListType,
      ["Store-1", "Store-2"],
      Users[0],
      [Users[1]]
    );

  public static readonly TestList CollaborationList =
    new(
      "Collaboration",
      BasicListItems,
      AutomationIdModel.DefaultListType,
      [],
      Users[1],
      [Users[0]]
    );

  public static readonly TestList ClipboardList =
    new(
      "Clipboard",
      StoreListItems,
      AutomationIdModel.ShoppingListType,
      ["Store-1", "Store-2"],
      Users[0],
      []
    );

  public static readonly TestList UserList =
    new("User", BasicListItems, AutomationIdModel.DefaultCategoryName, [], Users[2], []);

  public record TestUser(string Email, string Password, string Id);

  public record TestList(
    string Name,
    List<TestItem> Items,
    string ListType,
    List<string> Categories,
    TestUser Owner,
    List<TestUser> Collaborators
  );

  public record TestItem(string Name, string Category, int Quantity, bool IsImportant);
}
