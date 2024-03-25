using MongoDB.Bson;
using Realms;
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable RedundantExtendsListEntry
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Listem.Mobile.Models;

public partial class Item : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public List? List { get; set; }
    public Category? Category { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public bool IsImportant { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public string ToLoggableString()
    {
        return $"[RealmItem] {Name} {Id} in {List?.Name} (category: {Category?.Name}, quantity: {Quantity}, important: {IsImportant})";
    }
}
