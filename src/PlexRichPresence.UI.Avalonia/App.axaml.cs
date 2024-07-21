using System;
using System.Linq;
using System.Threading.Tasks;
using Akavache;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plex.Api.Factories;
using Plex.Library.Factories;
using Plex.ServerApi;
using Plex.ServerApi.Api;
using Plex.ServerApi.Clients;
using Plex.ServerApi.Clients.Interfaces;
using PlexRichPresence.DiscordRichPresence;
using PlexRichPresence.DiscordRichPresence.Rendering;
using PlexRichPresence.PlexActivity;
using PlexRichPresence.UI.Avalonia.Services;
using PlexRichPresence.UI.Avalonia.Views;
using PlexRichPresence.ViewModels;
using PlexRichPresence.ViewModels.Services;
using Serilog;

namespace PlexRichPresence.UI.Avalonia;

public class App : Application
{
    private readonly IServiceCollection _services = new ServiceCollection()
        .AddSingleton(new ClientOptions
        {
            Product = "Discord_Plex_Rich_Presence",
            DeviceName = Environment.MachineName,
            ClientId = "nDwkFkJCCJQEjq44TDaLJwKW54",
            Platform = "Desktop",
            Version = "v2"
        })
        .AddTransient<IPlexServerClient, PlexServerClient>()
        .AddTransient<IPlexAccountClient, PlexAccountClient>()
        .AddTransient<IPlexLibraryClient, PlexLibraryClient>()
        .AddTransient<IApiService, ApiService>()
        .AddTransient<IPlexFactory, PlexFactory>()
        .AddTransient<IPlexRequestsHttpClient, PlexRequestsHttpClient>()
        .AddSingleton<IStorageService>(
            new StorageService(StorageFolder))
        .AddSingleton<LoginPageViewModel>()
        .AddSingleton<ServersPageViewModel>()
        .AddSingleton<PlexActivityPageViewModel>()
        .AddSingleton<LoginPageViewModel>()
        .AddSingleton<IBrowserService, BrowserService>()
        .AddSingleton<IPlexActivityService, PlexActivityService>()
        .AddSingleton<IDiscordService, DiscordService>()
        .AddSingleton<IClock, Clock>()
        .AddSingleton<PlexSessionRenderingService>()
        .AddSingleton<PlexSessionRendererFactory>()
        .AddSingleton<PlexSessionMapper>()
        .AddLogging(loggingBuilder => { loggingBuilder.AddSerilog(); });

    private static readonly string StorageFolder =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.plexrichpresence";

    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ConfigureSerilog();


            ConfigureTheme();
            desktop.MainWindow = new MainWindow();
            var navigationFrame = desktop.MainWindow.FindControl<Frame>("navigationFrame");
            var navigationService = new NavigationService(navigationFrame);
            ConfigureNavigation(navigationService);

        }

        base.OnFrameworkInitializationCompleted();
    }

    
    
    private static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(path: $"{StorageFolder}/logs.txt", rollingInterval: RollingInterval.Hour)
            .CreateLogger();
    }

    private void ConfigureNavigation(NavigationService navigationService)
    {
        navigationService.RegisterPage("login", typeof(LoginPage));
        navigationService.RegisterPage("servers", typeof(ServersPage));
        navigationService.RegisterPage("activity", typeof(ActivityPage));
        _services.AddSingleton<INavigationService>(navigationService);
        _serviceProvider = _services.BuildServiceProvider();
        Resources[typeof(IServiceProvider)] = _serviceProvider;
        var storageService = _serviceProvider.GetService<IStorageService>()
                             ?? throw new InvalidOperationException("Can't get storage service from DI");
        
        Dispatcher.UIThread.Post(async () => await NavigateToFirstPage(storageService, navigationService));
    }

    private static void ConfigureTheme()
    {
        var faTheme = AvaloniaLocator
                          .Current
                          .GetService<FluentAvaloniaTheme>()
                      ?? throw new InvalidOperationException("Can't get theme from Avalonia Locator");
        faTheme.PreferSystemTheme = true;
        faTheme.CustomAccentColor = Color.FromRgb(0, 102, 204);
    }

    private void MinimizeIfNeeded()
    {
        string[] commandLineArgs = Environment.GetCommandLineArgs();
        if (commandLineArgs.Contains("--minimized") &&
            ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.Hide();
        }
    }

    private async Task NavigateToFirstPage(IStorageService storageService, INavigationService navigationService)
    {
        await storageService.Init();
        MinimizeIfNeeded();
        if (await storageService.ContainsKeyAsync("serverIp")
            && await storageService.ContainsKeyAsync("serverPort")
            && await storageService.ContainsKeyAsync("plex_token"))
        {
            await navigationService.NavigateToAsync("activity");
        }
        else if (await storageService.ContainsKeyAsync("plex_token"))
        {
            await navigationService.NavigateToAsync("servers");
        }
        else
        {
            await navigationService.NavigateToAsync("login");
        }
    }

    private void ReduceToTray_OnClick(object? _, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.Hide();
        }
    }

    private void Show_OnClick(object? _, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.Show();
        }
    }

    private void Exit_Onclick(object? _, EventArgs e)
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
        if (_serviceProvider is not null)
        {
            var storageService = _serviceProvider.GetService<IStorageService>()
                                 ?? throw new InvalidOperationException(
                                     "Can't get storage service from DI");
            var viewModel = _serviceProvider.GetService<PlexActivityPageViewModel>()
                            ?? throw new InvalidOperationException(
                                "Can't get storage service from DI");
            var logger = _serviceProvider.GetService<ILogger<App>>()
                         ?? throw new InvalidOperationException(
                             "Can't get logger from DI");
            var enabledIdleStatusStringValue = viewModel.EnableIdleStatus.ToString();
            storageService.PutAsync("enableIdleStatus", enabledIdleStatusStringValue);
            logger.Log(LogLevel.Information, "Saving setting enableIdleStatus to storage with value : {Value}", enabledIdleStatusStringValue);
        }

        BlobCache.Shutdown().Wait();
        desktop.Shutdown();
    }
}