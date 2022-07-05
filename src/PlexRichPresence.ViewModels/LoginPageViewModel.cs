using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Account;
using Plex.ServerApi.PlexModels.OAuth;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels;

[INotifyPropertyChanged]
public partial class LoginPageViewModel
{
    private readonly IPlexAccountClient plexClient;
    private readonly INavigationService navigationService;
    private readonly IStorageService storageService;
    private readonly IBrowserService browserService;

    public LoginPageViewModel(IPlexAccountClient plexClient, INavigationService navigationService, IStorageService storageService, IBrowserService browserService)
    {
        this.plexClient = plexClient;
        this.navigationService = navigationService;
        this.storageService = storageService;
        this.browserService = browserService;
    }

    [ObservableProperty] [AlsoNotifyCanExecuteFor(nameof(LoginWithCredentialsCommand))]
    private string login;

    [ObservableProperty] [AlsoNotifyCanExecuteFor(nameof(LoginWithCredentialsCommand))]
    private string password;

    [ICommand(AllowConcurrentExecutions = false, CanExecute = "CanLoginWithCredentials")]
    private async Task LoginWithCredentials()
    {
        PlexAccount account = await this.plexClient.GetPlexAccountAsync(this.Login, this.Password);
        await StoreTokenAndNavigateToServerPage(account.AuthToken);
    }

    private async Task StoreTokenAndNavigateToServerPage(string token)
    {
        await storageService.PutAsync("plex_token", token);
        await navigationService.NavigateToAsync("servers");
    }

    private bool CanLoginWithCredentials => !(string.IsNullOrEmpty(this.Login) || string.IsNullOrEmpty(this.password));

    [ICommand(AllowConcurrentExecutions = false)]
    private async Task LoginWithBrowser()
    {
        var oauthUrl = await this.plexClient.CreateOAuthPinAsync("");
        await this.browserService.OpenAsync(oauthUrl.Url);

        OAuthPin plexPin;
        while (true)
        {
            plexPin = await this.plexClient.GetAuthTokenFromOAuthPinAsync(oauthUrl.Id.ToString());
            if (string.IsNullOrEmpty(plexPin.AuthToken))
            {
                await Task.Delay(3000);
            }
            else break;
        }

        await StoreTokenAndNavigateToServerPage(plexPin.AuthToken);
    }
}