using System.Diagnostics.CodeAnalysis;
using Listem.Mobile.Services;
using MongoDB.Bson;
using Realms;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Listem.Mobile.Models;

[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
[SuppressMessage("ReSharper", "ReplaceAutoPropertyWithComputedProperty")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public partial class List : IRealmObject, IShareable
{
  [PrimaryKey]
  [MapTo("_id")]
  public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
  public string Name { get; set; } = null!;
  public string OwnedBy { get; set; } = null!;
  public ISet<string> SharedWith { get; } = null!;
  public string ListType { get; set; } = null!;
  public DateTimeOffset UpdatedOn { get; set; }
  public IList<Category> Categories { get; } = null!;
  public bool IsDraft { get; set; } = true;

  [Backlink(nameof(Item.List))]
  public IQueryable<Item> Items { get; }

  public bool IsMine => OwnedBy == RealmService.User.Id;
  public bool IsShared => SharedWith.Count > 0;
  public bool IsSharedWithMe =>
    RealmService.User.EmailAddress != null && SharedWith.Contains(RealmService.User.EmailAddress);
  public bool IsSharedByMe => IsMine && IsShared;

  public List UntrackedCopy()
  {
    var list = new List
    {
      Id = Id,
      Name = Name,
      OwnedBy = OwnedBy,
      ListType = ListType,
      UpdatedOn = UpdatedOn,
      IsDraft = IsDraft
    };
    foreach (var s in SharedWith)
    {
      list.SharedWith.Add(s);
    }
    foreach (var category in Categories)
    {
      list.Categories.Add(category);
    }
    return list;
  }

  public override string ToString()
  {
    return Name;
  }

  public string ToLog()
  {
    var sharedWith = SharedWith.Count > 0 ? string.Join(", ", SharedWith) : "";
    return $"[List] '{Name}' {Id}, type: {ListType}, owned by: {OwnedBy}, shared with: [{sharedWith}], last updated: {UpdatedOn.ToLocalTime()}";
  }
}
