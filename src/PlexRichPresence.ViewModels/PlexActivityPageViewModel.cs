using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels;

[INotifyPropertyChanged]
public partial class PlexActivityPageViewModel
{
    private IPlexActivityService plexActivityService;
    private IStorageService storageService;
    private INavigationService navigationService;
    private string userToken;
    
    [ObservableProperty] private bool isConnected = false;
    [ObservableProperty] private string currentActivity = "Idle";
    [ObservableProperty] private string plexServerIp;
    [ObservableProperty] private int plexServerPort;
    
    public PlexActivityPageViewModel(IPlexActivityService plexActivityService, IStorageService storageService, INavigationService navigationService)
    {
        this.plexActivityService = plexActivityService;
        this.storageService = storageService;
        this.navigationService = navigationService;
        this.plexActivityService.OnDisconnection += (_, _) => IsConnected = false;
        this.plexActivityService.OnActivityUpdated += (_, args) =>
        {
            var activity = (args as IPlexActivityService.PlexActivityEventArg).CurrentActivity;
            CurrentActivity = $"Playing : {activity}";
        };
    }

    [RelayCommand]
    public async Task GetDataFromStorage()
    {
        this.PlexServerIp = await this.storageService.GetAsync("serverIp");
        this.PlexServerPort = int.Parse(await this.storageService.GetAsync("serverPort"));
        this.userToken = await this.storageService.GetAsync("plex_token");
    }

    [RelayCommand]
    public void Connect()
    {
        this.plexActivityService.Connect(this.PlexServerIp, this.PlexServerPort, this.userToken, false);
        this.IsConnected = true;
    }

    [RelayCommand]
    public void Disconnect()
    {
        this.plexActivityService.Disconnect();
        this.IsConnected = false;
    }

    [RelayCommand]
    public async Task ChangeServer()
    {
        await this.navigationService.NavigateToAsync("servers");
    }
}