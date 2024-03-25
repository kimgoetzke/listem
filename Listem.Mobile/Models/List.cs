using Listem.Mobile.Services;
using MongoDB.Bson;
using Realms;
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ReplaceAutoPropertyWithComputedProperty
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Listem.Mobile.Models;

public partial class List : IRealmObject
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

    public bool IsSharedWithMe
    {
        get
        {
            if (RealmService.User.Id == null)
                return false;

            return SharedWith.Contains(RealmService.User.Id) && IsMine;
        }
    }

    public bool IsAccessibleToMe => IsSharedWithMe || IsMine;

    public override string ToString()
    {
        return Name;
    }

    public string ToLoggableString()
    {
        return $"[RealmList] '{Name}' {Id}, type: '{ListType}', last updated: {UpdatedOn}";
    }
}
