using System.Diagnostics.CodeAnalysis;
using Listem.Mobile.Services;
using MongoDB.Bson;
using Realms;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Listem.Mobile.Models;

[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
[SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ReplaceAutoPropertyWithComputedProperty")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public partial class Item : IRealmObject, IShareable
{
  [PrimaryKey]
  [MapTo("_id")]
  public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
  public string Name { get; set; }
  public string OwnedBy { get; set; } = RealmService.User.Id!;
  public ISet<string> SharedWith { get; } = null!;
  public List? List { get; set; }
  public Category? Category { get; set; }
  public int Quantity { get; set; }
  public bool IsImportant { get; set; }
  public bool IsDraft { get; set; } = true;
  public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.Now;

  public override string ToString()
  {
    return Name;
  }

  public string ToLog()
  {
    var sharedWith = SharedWith.Count > 0 ? string.Join(", ", SharedWith) : "";
    return $"[Item] '{Name}' {Id} in '{List?.Name}', category: {Category?.Name}, quantity: {Quantity}, important: {IsImportant}, owned by: {OwnedBy}, shared with: {sharedWith}, last updated: {UpdatedOn}";
  }
}
