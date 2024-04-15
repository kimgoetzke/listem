using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using Realms;

namespace Listem.Mobile.Models;

[SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public partial class UserData : IRealmObject
{
  [PrimaryKey]
  [MapTo("_id")]
  public ObjectId Id { get; set; }
  public string Email { get; set; }

  public string ToLog()
  {
    return "[CustomUserData] Id: " + Id + ", email: " + Email;
  }
}
