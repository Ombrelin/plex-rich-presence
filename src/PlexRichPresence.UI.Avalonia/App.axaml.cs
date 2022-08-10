using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Plex.Api.Factories;
using Plex.Library.Factories;
using Plex.ServerApi;
using Plex.ServerApi.Api;
using Plex.ServerApi.Clients;
using Plex.ServerApi.Clients.Interfaces;
using PlexRichPresence.UI.Avalonia.Services;
using PlexRichPresence.UI.Avalonia.ViewModels;
using PlexRichPresence.UI.Avalonia.Views;
using PlexRichPresence.ViewModels;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.UI.Avalonia;

public partial class App : Application
{
    private readonly IServiceCollection services = new ServiceCollection()
        .AddSingleton(new ClientOptions
        {
            Product = "Discord_Plex_Rich_Presence",
            DeviceName = Environment.MachineName,
            ClientId = "nDwkFkJCCJQEjq44TDaLJwKW54",
            Platform = "Desktop",
            Version = "v1"
        })
        .AddTransient<IPlexServerClient, PlexServerClient>()
        .AddTransient<IPlexAccountClient, PlexAccountClient>()
        .AddTransient<IPlexLibraryClient, PlexLibraryClient>()
        .AddTransient<IApiService, ApiService>()
        .AddTransient<IPlexFactory, PlexFactory>()
        .AddTransient<IPlexRequestsHttpClient, PlexRequestsHttpClient>()
        .AddSingleton<IStorageService, StorageService>()
        .AddLogging()
        .AddSingleton<LoginPageViewModel>();
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            var navigationFrame = desktop.MainWindow.FindControl<Frame>("navigationFrame");
            var navigationService = new NavigationService(navigationFrame);
            navigationService.RegisterPage("login", typeof(LoginPage));
            navigationService.RegisterPage("servers", typeof(ServersPage));

            services.AddSingleton<INavigationService>(navigationService);
            this.Resources[typeof(IServiceProvider)] = services.BuildServiceProvider();
            navigationService.NavigateToAsync("login");
        }

        base.OnFrameworkInitializationCompleted();
    }
}