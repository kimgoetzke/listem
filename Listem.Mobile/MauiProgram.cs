using CommunityToolkit.Maui;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Listem.Mobile.ViewModel;
using Listem.Mobile.Views;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Listem.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u4}] [XXX] {Message:lj}{NewLine}{Exception}"
            )
            .MinimumLevel.Information()
            .WriteTo.Debug(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u4}] [XXX] {Message:lj}{NewLine}{Exception}"
            )
            .MinimumLevel.Information()
            .WriteTo.Sink<AndroidLogSink>()
            .CreateLogger();

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
        builder.Services.AddSingleton<IListService, ListService>();
        builder.Services.AddSingleton<ICategoryService, CategoryService>();
        builder.Services.AddSingleton<IItemService, ItemService>();
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IClipboardService, ClipboardService>();

#if DEBUG
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
#endif

        return builder;
    }

    private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<EditListViewModel>();
        builder.Services.AddTransient<ListViewModel>();
        builder.Services.AddTransient<DetailViewModel>();
        return builder;
    }

    private static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<StartPage>();
        builder.Services.AddSingleton<SignInPage>();
        builder.Services.AddSingleton<SignUpPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<EditListPage>();
        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<ListPage>();
        return builder;
    }
}
