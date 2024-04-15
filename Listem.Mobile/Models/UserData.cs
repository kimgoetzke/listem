using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using Realms;

namespace Listem.Mobile.Models;

[SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public partial class UserData : IRealmObject
{
  [PrimaryKey]
  [MapTo("_id")]
  public ObjectId Id { get; set; }
  public string Email { get; set; } = null!;

  public string ToLog()
  {
    return "[CustomUserData] Id: " + Id + ", email: " + Email;
  }
}
