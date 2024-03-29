namespace Listem.Mobile.Models;

public record KnownUser(string Id, string Email)
{
  public override string ToString()
  {
    return Email;
  }

  public string ToLog()
  {
    return $"[KnownUser] '{Id}' {Email}";
  }
}
