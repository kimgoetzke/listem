using CommunityToolkit.Mvvm.Messaging.Messages;
using Listem.Mobile.Models;

namespace Listem.Mobile.Events;

public sealed class ListModifiedMessage(ObservableList list)
  : ValueChangedMessage<ObservableList>(list);
