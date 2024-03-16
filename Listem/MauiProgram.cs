using CommunityToolkit.Maui;
using Listem.Services;
using Listem.ViewModel;
using Listem.Views;
using Microsoft.Extensions.Logging;
using MainPage = Listem.Views.MainPage;

namespace Listem;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        return MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Mulish-Black.ttf", "MulishBlack");
                fonts.AddFont("Mulish-Bold.ttf", "MulishBold");
                fonts.AddFont("Mulish-SemiBold.ttf", "MulishSemiBold");
                fonts.AddFont("Mulish-Regular.ttf", "MulishRegular");
                fonts.AddFont("Mulish-Light.ttf", "MulishLight");
            })
            .UseMauiCommunityToolkit()
            .RegisterServices()
            .RegisterViewModels()
            .RegisterViews()
            .Build();
    }

    private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<IDatabaseProvider, DatabaseProvider>();
        builder.Services.AddSingleton<IItemListService, ItemListService>();
        builder.Services.AddSingleton<ICategoryService, CategoryService>();
        builder.Services.AddSingleton<IItemService, ItemService>();
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddHttpClient(
            Constants.HttpClientName,
            client => client.BaseAddress = new Uri(Constants.BaseUrlLocalhost)
        );
        builder.Services.AddSingleton<IClipboardService, ClipboardService>();
        builder.Services.AddLogging();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder;
    }

    private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<EditListViewModel>();
        builder.Services.AddTransient<ListViewModel>();
        builder.Services.AddTransient<DetailViewModel>();
        return builder;
    }

    private static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<SignInPage>();
        builder.Services.AddSingleton<SignUpPage>();
        builder.Services.AddTransient<EditListPage>();
        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<ListPage>();
        return builder;
    }
}
