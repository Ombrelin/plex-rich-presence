using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Account;
using Plex.ServerApi.PlexModels.OAuth;
using System.Diagnostics;

namespace PlexRichPresence.UI.ViewModels;

[INotifyPropertyChanged]
public partial class LoginPageViewModel
{
    private readonly IPlexAccountClient plexClient;

    public LoginPageViewModel(IPlexAccountClient plexClient)
    {
        this.plexClient = plexClient;
    }

    [ObservableProperty]
    [AlsoNotifyCanExecuteFor(nameof(LoginWithCredentialsCommand))]
    private string login;

    [ObservableProperty]
    [AlsoNotifyCanExecuteFor(nameof(LoginWithCredentialsCommand))]
    private string password;

    [ICommand(AllowConcurrentExecutions = false, CanExecute = "CanLoginWithCredentials")]
    public async Task LoginWithCredentials()
    {
        PlexAccount account = await this.plexClient.GetPlexAccountAsync(this.Login, this.Password);
        await SecureStorage.Default.SetAsync("plex_token", account.AuthToken);
        await Shell.Current.GoToAsync("servers");
    }

    public bool CanLoginWithCredentials => !(string.IsNullOrEmpty(this.Login) || string.IsNullOrEmpty(this.password));

    [ICommand(AllowConcurrentExecutions = false)]
    public async Task LoginWithBrowser()
    {
        var oauthUrl = await this.plexClient.CreateOAuthPinAsync("");
        await Browser.Default.OpenAsync(oauthUrl.Url, BrowserLaunchMode.SystemPreferred);

        OAuthPin plexPin;
        while(true){
            plexPin = await this.plexClient.GetAuthTokenFromOAuthPinAsync(oauthUrl.Id.ToString());
            if (string.IsNullOrEmpty(plexPin.AuthToken))
            {
                await Task.Delay(3000);
            }
            else break;
        }
        await SecureStorage.Default.SetAsync("plex_token", plexPin.AuthToken);
      
        await Shell.Current.GoToAsync("servers");
    }

    [ICommand]
    public async Task CheckToken()
    {
        await Task.Delay(1);
        string plexToken = await SecureStorage.Default.GetAsync("plex_token");
        if (!string.IsNullOrEmpty(plexToken)){
            try
            {
                await Shell.Current.GoToAsync("servers");

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            
        }
    }
}