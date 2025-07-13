using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Listem.Mobile.Events;

public sealed class ItemRemovedFromListMessage(ItemChangedDto value)
  : ValueChangedMessage<ItemChangedDto>(value);
