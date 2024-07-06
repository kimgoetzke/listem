# `AutomationId`s in `ListView.xaml` trigger `RealmInvalidObjectException` when using MongoDB Realm

The purpose of this branch is to serve as a reference point on how to trigger a `RealmInvalidObjectException` when using
`AutomationId`s in `ListView.xaml`. The merge existence of `AutomationId`s will cause the exception to be thrown under
certain circumstances.

## Issue

When using `AutomationId`s in a ListView, a `Realms.Exceptions.RealmInvalidObjectException` is thrown when swiping an
item to the right in order to delete it. The swipe action will attempt to remove the item from the Realm. However, the
exception, which will immediately crash the app, is thrown instead.

As a result, automated UI testing cannot be used for testing lists with MongoDB Realm in this application.

> [!IMPORTANT]
> This bug does not occur in `Debug` mode. It only occurs in `Release` mode.

## Steps to reproduce

1. Start the app in `Release` mode (either emulator or physical device).
2. Login with `someone@example` and `Password1!`.
3. Create a new list and open it.
4. Add several items to the list.
5. Swipe an item to the right in order to delete it.
6. Repeat the swipe action on different items until the exception occurs.

The exception will usually not occur on the first swipe.

## Steps to stop the issue from occuring

1. Remove all occurrences of `AutomationId` from the `ListView.xaml`.
2. Follow the steps to reproduce the issue again and notice that the exception no longer occurs.

## Further information

https://www.mongodb.com/community/forums/t/realminvalidobjectexception-in-release-but-not-in-debug-mode/285108