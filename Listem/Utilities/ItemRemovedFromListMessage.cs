using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Listem.Utilities;

public sealed class ItemRemovedFromListMessage(ItemChangedDto value)
    : ValueChangedMessage<ItemChangedDto>(value);
