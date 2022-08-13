using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels;

[INotifyPropertyChanged]
public partial class PlexActivityViewModel
{
    private IPlexActivityService plexActivityService;
    private IStorageService storageService;
    private string userToken;
    
    [ObservableProperty] private bool isConnected = false;
    [ObservableProperty] private string currentActivity = "Idle";
    [ObservableProperty] private string plexServerIp;
    [ObservableProperty] private int plexServerPort;
    
    public PlexActivityViewModel(IPlexActivityService plexActivityService, IStorageService storageService)
    {
        this.plexActivityService = plexActivityService;
        this.storageService = storageService;
        
        this.plexActivityService.OnActivityUpdated += (source, args) =>
        {
            var activity = (args as IPlexActivityService.PlexActivityEventArg).CurrentActivity;
            this.CurrentActivity = $"Playing : {activity}";
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
        this.plexActivityService.Connect(this.PlexServerIp, this.PlexServerPort, this.userToken);
        this.IsConnected = true;
    }

    [RelayCommand]
    public void Disconnect()
    {
        this.plexActivityService.Disconnect();
    }
}