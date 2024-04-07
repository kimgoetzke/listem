namespace Listem.Mobile.UITests;

public abstract class TestData
{
  private static List<TestItem> List0Items { get; } =
    [
      new TestItem("Item-0", AutomationIdModel.DefaultCategoryName, 1, false),
      new TestItem("Item-1", "Store-1", 1, false),
      new TestItem("Item-2", "Store-1", 1, true),
      new TestItem("Item-3", "Store-2", 2, true),
      new TestItem("Item-4", "Store-2", 5, true),
      new TestItem("Item-5", AutomationIdModel.DefaultCategoryName, 1, false)
    ];

  private static List<TestItem> List1Items { get; } =
    [
      new TestItem("Item-0", AutomationIdModel.DefaultCategoryName, 1, false),
      new TestItem("Item-1", "Category-1", 1, false),
      new TestItem("Item-1", "Category-2", 1, false)
    ];

  public static List<TestList> Lists { get; } =
    [
      new TestList(
        "List-0",
        List0Items,
        AutomationIdModel.ShoppingListType,
        ["Store-1", "Store-2"],
        ["Collaborator-1"]
      ),
      new TestList(
        "List-1",
        List1Items,
        AutomationIdModel.DefaultListType,
        ["Category-3", "Category-4"],
        ["Collaborator-2"]
      )
    ];

  public static List<TestUser> Users { get; } = [new TestUser("someone@example", "Password1!")];

  public record TestUser(string Email, string Password);

  public record TestList(
    string Name,
    List<TestItem> Items,
    string ListType,
    List<string> Categories,
    List<string> Collaborators
  );

  public record TestItem(string Name, string Category, int Quantity, bool IsImportant);
}
