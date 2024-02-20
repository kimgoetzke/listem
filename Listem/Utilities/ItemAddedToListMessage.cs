using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Listem.Utilities;

public sealed class ItemAddedToListMessage(ItemChangedDto value)
    : ValueChangedMessage<ItemChangedDto>(value);
