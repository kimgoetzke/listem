using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Listem.Events;

public sealed class ItemAddedToListMessage(ItemChangedDto value)
    : ValueChangedMessage<ItemChangedDto>(value);
