using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Listem.Mobile.Events;

public sealed class UserIsSignedInMessage(bool isSignedIn) : ValueChangedMessage<bool>(isSignedIn);
