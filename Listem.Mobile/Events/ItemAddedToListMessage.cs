using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Listem.Mobile.Events;

public sealed class ItemAddedToListMessage(ItemChangedDto value)
  : ValueChangedMessage<ItemChangedDto>(value);
