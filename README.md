# The Listem Project

This repository contains a simple, minimalist todo list Android app written in C# using .NET 8 MAUI, the
CommunityToolkit, and SQLite. The project uses Appium and NUnit for UI testing. 

## How to build develop

1. Set environment variables for builds and running tests
    1. `ANDROID_HOME` - the absolute path of the Android SDK
    2. `SHOPPING_LIST_DEBUG_APK` - the absolute path of the debug APK
    3. `SHOPPING_LIST_RELEASE_APK` - the absolute path of the release APK

## How to build the APK

Create APK with:

```shell
dotnet publish -f:net8.0-android -c:Release /p:AndroidSdkDirectory=$env:ANDROID_HOME
```

This assumes that the Android SDK is installed and the `ANDROID_HOME` environment variable is set.

APK file can then be found in `ShoppingList\bin\Release\net8.0-android\publish\` and installed directly on any Android
phone.

## How to run tests

_Note: Currently, I am unable to get Appium to install the APK correctly on the emulator. The only way to make the app
start during tests is to first install the APK on the device, close the welcome popup, and then run the tests. If the
APK is ever installed by Appium, the device needs to be wiped and the APK installed without Appium for the tests to
pass again._

To run the tests:
1. Install the APK on the device/emulator
2. Launch the app to close the welcome popup
3. Close it again
4. Run the tests via your IDE or `donet test`
