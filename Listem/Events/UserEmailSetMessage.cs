using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Listem.Events;

public sealed class UserEmailSetMessage(string email) : ValueChangedMessage<string>(email);
