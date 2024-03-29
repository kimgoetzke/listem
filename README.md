# The Listem Project With An ASP.NET Core Backend

This repository contains an Android application and the relevant backend. The first one is a simple, minimalist to-do
list Android app written in C# using .NET 8 MAUI, the CommunityToolkit, and SQLite. It also contains the backend for
this app, also written in C# using ASP.NET Core, Entity Framework Core, and ASP Core Authentication.

> [!IMPORTANT]  
> This branch of the project features an ASP.NET Core backend service and the .NET MAUI mobile application. However, the
> application does not sync data between offline and online mode. At this stage, the two are completely separate data
> sets. The app is also configured for localhost only. These features would need to be added. Also, no test were been
> written because implementing some form of the data conflict management was expected to cause numerous substantial
> changes to the architecture to both front- and backend, after which testing would be appropriate.

The goal for creating this branch was to explore web development with ASP.NET Core.

![Screenshots PNG](./assets/screenshots.png)

## MAUI Android application

> [!NOTE]  
> The only changes to the original project (at that same point in time) was the introduction of `ApiService`s
> and `ServiceResolver`s to handle the communication either the backend or the local SQLite database.

### Overview

- A super basic, minimalist to-do list app targeting Android
- Users can register and log in to save their lists and categories in the cloud
- Alternatively, the app can be used without account/connection, storing all data in a SQLite database on the device
- Lists can be somewhat customised by adding categories or list types (e.g. changing to shopping list exposes a
  quantity control)
- A list's content can be exported to the clipboard as text
- List items can be imported from a comma-separated string from the clipboard and merged with the current list
- Native confirmation prompts are used for destructive actions
- Theming hasn't been implemented this time but can be enabled by configuring `DarkTheme.xaml` and exposing a control to
  change theme
- Icons used are CC0 from [iconsDB.com](https://www.iconsdb.com/) or self-made
- Colour scheme and topography inspired by Mailin
  Hülsmann's [Tennis App - UX/UI Design Case Study](https://www.behance.net/gallery/124361333/Tennis-App-UXUI-Design-Case-Study)

### How to configure your environment for development

1. Set environment variables for builds and running tests
    1. `ANDROID_HOME` - the absolute path of the Android SDK
    2. `SHOPPING_LIST_DEBUG_APK` - the absolute path of the debug APK, used by UI tests only
    3. `SHOPPING_LIST_RELEASE_APK` - the absolute path of the release APK, used by UI tests only
2. Run `dotnet restore` in the base directory to restore all dependencies

### How to build the APK

Create APK with:

```shell
cd Listem
dotnet publish -f:net8.0-android -c:Release /p:AndroidSdkDirectory=$env:ANDROID_HOME
```

This assumes that the Android SDK is installed and the `ANDROID_HOME` environment variable is set.

APK file can then be found in `ShoppingList\bin\Release\net8.0-android\publish\` and installed directly on any Android
phone.

## Backend

### Overview

- A minimal REST API for managing lists, categories, and items
- Token based authentication flow
- Separate Entity Framework Core database contexts for each entity type
- Middleware that creates and makes available a request context (e.g. containing the user id) for each authenticated
  endpoint
- Custom logging middleware
- Custom exception handling middleware

### How to configure the backend for development

1. Run `dotnet restore` in the base directory to restore all dependencies, if you haven't already done so.
2. The first time running the application, you'll need to create the database and run the migrations. This can be done
   by running the following command from the root of the repository:

    ```shell
    cd Listem.API && dotnet ef migrations add InitialCreate --context ListDbContext --output-dir Migrations/Lists  && dotnet ef database update --context ListDbContext && dotnet ef migrations add InitialCreate --context CategoryDbContext --output-dir Migrations/Categories && dotnet ef database update --context CategoryDbContext && dotnet ef migrations add InitialCreate --context ItemDbContext --output-dir Migrations/Items && dotnet ef database update --context ItemDbContext && dotnet ef migrations add InitialCreate --context UserDbContext --output-dir Migrations/Users && dotnet ef database update --context UserDbContext
    ```

   Alternatively, if you want to run each command separately:

    ```shell
    cd Listem.API
    dotnet ef migrations add InitialCreate --context ListDbContext --output-dir Migrations/Lists
    dotnet ef database update --context ListDbContext
    dotnet ef migrations add InitialCreate --context CategoryDbContext --output-dir Migrations/Categories 
    dotnet ef database update --context CategoryDbContext
    dotnet ef migrations add InitialCreate --context ItemDbContext --output-dir Migrations/Items
    dotnet ef database update --context ItemDbContext
    dotnet ef migrations add InitialCreate --context UserDbContext --output-dir Migrations/Users 
    dotnet ef database update --context UserDbContext
    ```

3. You can use the Postman collection in the `/assets` directory to test the API.