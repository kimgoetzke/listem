using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Listem.Mobile.Events;

public sealed class UserEmailSetMessage(string email) : ValueChangedMessage<string>(email);
