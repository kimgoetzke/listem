namespace Listem.Mobile.UITests;

public static class AutomationIdModel
{
  public const string DefaultCategoryName = "None";
  public const string DefaultListType = "Standard";
  public const string ShoppingListType = "Shopping";

  public static class StartPage
  {
    public const string SignInButton = "SignInButton";
  }

  public static class SignInPage
  {
    public const string EmailEntry = "EmailEntry";
    public const string PasswordEntry = "PasswordEntry";
    public const string SignInButton = "SignInButton";
  }

  public static class MainPage
  {
    public const string AddListButton = "AddListButton";
    public const string MenuButton = "MenuButton";

    public static class List
    {
      public const string ListTitle = "ListTitle_";
      public const string EditButton = "EditList_";
      public const string EmptyListLabel = "EmptyListLabel_";
      public const string DeleteButton = "DeleteButton_";
      public const string ExitButton = "ExitButton_";

      public static class Tags
      {
        public const string Shared = "SharedTag_";
        public const string Collaborator = "CollaboratorTag_";
        public const string Owner = "OwnerTag_";
      }
    }

    public static class Menu
    {
      public const string SignOutButton = "SignOutButton";
    }
  }

  public static class EditListPage
  {
    public const string BackButton = "BackButton";
    public const string ListNameEntry = "ListNameEntry";
    public const string ListTypePicker = "ListTypePicker";
    public const string AddCategoryButton = "AddCategoryButton";
    public const string ResetCategoriesButton = "ResetCategoriesButton";
    public const string ShareButton = "ShareButton";
    public const string UnshareButton = "UnshareButton";

    public static class Categories
    {
      public const string Label = "CollectionItem_";
    }

    public static class Collaborators
    {
      public const string Label = "CollectionItem_";
    }
  }

  public static class ListPage
  {
    public const string BackButton = "BackButton";
    public const string CategoryPicker = "ListPageCategoryPicker";
    public const string AddButton = "ListPageAddButton";
    public const string EntryField = "ListPageEntryField";
    public const string QuantityStepper = "ListPageQuantityStepper";
    public const string IsImportantSwitch = "ListPageIsImportantSwitch";

    public static class Item
    {
      public const string Label = "Label_";
      public const string CategoryTag = "CategoryTag_";
      public const string QuantityLabel = "QuantityLabel_";
      public const string IsImportantIcon = "IsImportantIcon_";
      public const string DoneBox = "DoneBox_";
    }
  }

  public static class StickyEntry
  {
    public const string EntryField = "StickyEntryField";
    public const string SubmitButton = "StickyEntrySubmit";
    public const string CancelButton = "StickyEntryCancel";
  }

  public static class Tag
  {
    public const string Label = "TagLabel";
  }
}
