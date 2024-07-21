using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Account;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels;

[INotifyPropertyChanged]
public partial class ServersPageViewModel
{
    public ObservableCollection<AccountServer> Servers { get; set; } = new();
    private readonly IPlexAccountClient _plexAccountClient;
    private readonly IStorageService _storageService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<PlexActivityPageViewModel> _logger;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _thumbnailUrl = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateServerSelectionCommand))]
    private AccountServer? _selectedServer;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateServerSelectionCommand))]
    private string _customServerIp = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateServerSelectionCommand))]
    private string _customServerPort = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateServerSelectionCommand))]
    private bool _useCustomServer;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateServerSelectionCommand))]
    private bool _isCustomServerOwned;

    public bool CanValidate => SelectedServer is not null || (UseCustomServer && !string.IsNullOrEmpty(CustomServerIp) && !string.IsNullOrEmpty(CustomServerPort));

    public ServersPageViewModel(IPlexAccountClient plexAccountClient, IStorageService storageService, INavigationService navigationService, ILogger<PlexActivityPageViewModel> logger)
    {
        _plexAccountClient = plexAccountClient;
        _storageService = storageService;
        _navigationService = navigationService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task GetData()
    {
        try
        {
            await Task.WhenAll(GetServers(), GetUsernameAndThumbnail());
        }
        catch (ApplicationException e)
        {
            if (e.Message is "Unsuccessful response from 3rd Party API")
            {
                _logger.LogError("No PLEX token is settings going back to login screen");
                await _navigationService.NavigateToAsync("login");
            }
        }
    }

    private async Task GetUsernameAndThumbnail()
    {
        var plexToken = await _storageService.GetAsync("plex_token");
        var account = await _plexAccountClient.GetPlexAccountAsync(plexToken);
        Username = account.Username;
        ThumbnailUrl = account.Thumb;
    }

    private async Task GetServers()
    {
        var plexToken = await _storageService.GetAsync("plex_token");
        var serverContainer = await _plexAccountClient.GetAccountServersAsync(plexToken);

        foreach (var server in serverContainer.Servers)
        {
            Servers.Add(server);
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false, CanExecute = "CanValidate")]
    private async Task ValidateServerSelection()
    {
        var (selectedServerIp, selectedServerPort, isSelectedServerOwned) = GetSelectedServerInfo();

        await _storageService.PutAsync("serverIp", selectedServerIp);
        await _storageService.PutAsync("serverPort", selectedServerPort.ToString());
        await _storageService.PutAsync("isServerOwned", isSelectedServerOwned.ToString());

        await _navigationService.NavigateToAsync("activity");
    }

    private (string, int, bool) GetSelectedServerInfo()
    {
        if (UseCustomServer)
        {
            return (CustomServerIp, int.Parse(CustomServerPort), IsCustomServerOwned);
        }

        if (SelectedServer is not null)
        {
            return (SelectedServer.Address, SelectedServer.Port, SelectedServer.Owned == 1);
        }

        throw new ArgumentException("No server selected");
    }

    [RelayCommand]
    private async Task LogOut()
    {
        await _storageService.RemoveAsync("plex_token");
        await _navigationService.NavigateToAsync("login");
    }
}