using Realms;

// ReSharper disable RedundantExtendsListEntry

namespace Listem.Mobile.Models;

public partial class Category : IEmbeddedObject
{
    public string Name { get; set; } = null!;

    public string ToLoggableString()
    {
        return $"[RealmCategory] {Name}";
    }
}
