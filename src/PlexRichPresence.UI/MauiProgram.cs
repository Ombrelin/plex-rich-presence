using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using PlexRichPresence.UI.Pages;

namespace PlexRichPresence.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });


        // App Shell
        builder.Services.AddTransient<AppShell>();

        // Services
        builder.Services.AddSingleton<App>();

        // Pages
        builder.Services.AddTransient<LoginPage>();

        return builder.Build();

    }
}