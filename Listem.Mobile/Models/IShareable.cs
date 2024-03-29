namespace Listem.Mobile.Models;

public interface IShareable
{
  string OwnedBy { get; set; }
  ISet<string> SharedWith { get; }
  DateTimeOffset UpdatedOn { get; set; }
}
