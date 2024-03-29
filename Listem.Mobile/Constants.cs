namespace Listem.Mobile;

public static class Constants
{
  public const string LoggerTag = "Listem";
  public const string User = "CurrentUser";
  public const string KnownUsers = "KnownUsers";
  public const string LocalEncryptionKey = "LocalEncryptionKey";
  public const string DefaultCategoryName = "None";

  // Error messages
  public const string UnauthorisedMessage = "You are not authorised to make this request";
  public const string ForbiddenMessage = "You are not allowed to make this request";
  public const string DefaultMessage = "Sorry, something went wrong - please try again";
  public const string TimeoutMessage = "Sorry, can't reach the server - please try again later";
}
