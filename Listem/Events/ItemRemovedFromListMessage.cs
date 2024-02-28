using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Listem.Events;

public sealed class ItemRemovedFromListMessage(ItemChangedDto value)
    : ValueChangedMessage<ItemChangedDto>(value);
