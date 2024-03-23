using CommunityToolkit.Mvvm.Messaging.Messages;
using Listem.Mobile.Models;

namespace Listem.Mobile.Events;

public sealed class UserStatusChangedMessage(User user) : ValueChangedMessage<User>(user);
