using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Logging;
using Plex.Api.Factories;
using Plex.Library.Factories;
using Plex.ServerApi;
using Plex.ServerApi.Api;
using Plex.ServerApi.Clients;
using Plex.ServerApi.Clients.Interfaces;
using PlexRichPresence.UI.Pages;
using PlexRichPresence.UI.ViewModels;

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

// ...
#if DEBUG
        builder.Services.AddLogging(configure =>
        {
            configure.AddDebug();
        });
#endif
        // App Shell
        builder.Services.AddTransient<AppShell>();

        // Services
        var apiOptions = new ClientOptions
        {
            Product = "Discord_Plex_Rich_Presence",
            DeviceName = Environment.MachineName,
            ClientId = "nDwkFkJCCJQEjq44TDaLJwKW54",
            Platform = "Desktop",
            Version = "v1"
        };


        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton(apiOptions);
        builder.Services.AddTransient<IPlexServerClient, PlexServerClient>();
        builder.Services.AddTransient<IPlexAccountClient, PlexAccountClient>();
        builder.Services.AddTransient<IPlexLibraryClient, PlexLibraryClient>();
        builder.Services.AddTransient<IApiService, ApiService>();
        builder.Services.AddTransient<IPlexFactory, PlexFactory>();
        builder.Services.AddTransient<IPlexRequestsHttpClient, PlexRequestsHttpClient>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ServersPage>();

        // ViewModels
        builder.Services.AddTransient<LoginPageViewModel>();
        builder.Services.AddTransient<ServersPageViewModel>();

        return builder.Build();
    }
}