using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.OAuth;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels;

[INotifyPropertyChanged]
public partial class LoginPageViewModel
{
    private readonly IPlexAccountClient _plexClient;
    private readonly INavigationService _navigationService;
    private readonly IStorageService _storageService;
    private readonly IBrowserService _browserService;
    private readonly IClock _clock;

    public LoginPageViewModel(IPlexAccountClient plexClient, INavigationService navigationService,
        IStorageService storageService, IBrowserService browserService, IClock clock)
    {
        _plexClient = plexClient;
        _navigationService = navigationService;
        _storageService = storageService;
        _browserService = browserService;
        _clock = clock;
    }

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(LoginWithCredentialsCommand))]
    private string _login = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(LoginWithCredentialsCommand))]
    private string _password = string.Empty;

    [RelayCommand(AllowConcurrentExecutions = false, CanExecute = "CanLoginWithCredentials")]
    private async Task LoginWithCredentials()
    {
        var account = await _plexClient.GetPlexAccountAsync(Login, Password);
        await StoreTokenAndNavigateToServerPage(account.AuthToken, account.Username);
    }

    private async Task StoreTokenAndNavigateToServerPage(string token, string userName)
    {
        await _storageService.PutAsync("plexUserName", userName);
        await _storageService.PutAsync("plex_token", token);
        await _navigationService.NavigateToAsync("servers");
    }

    public bool CanLoginWithCredentials => !(string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(_password));

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task LoginWithBrowser()
    {
        var oauthUrl = await OpenBrowserWitnPin();
        var plexPin = await WaitForBrowserLogin(oauthUrl);

        var account = await _plexClient.GetPlexAccountAsync(plexPin.AuthToken);
        await StoreTokenAndNavigateToServerPage(plexPin.AuthToken, account.Username);
    }

    private async Task<OAuthPin> WaitForBrowserLogin(OAuthPin oauthUrl)
    {
        OAuthPin plexPin;
        while (true)
        {
            plexPin = await _plexClient.GetAuthTokenFromOAuthPinAsync(oauthUrl.Id.ToString());
            if (string.IsNullOrEmpty(plexPin.AuthToken))
            {
                await _clock.Delay(TimeSpan.FromSeconds(2));
            }
            else break;
        }

        return plexPin;
    }

    private async Task<OAuthPin> OpenBrowserWitnPin()
    {
        var oauthUrl = await _plexClient.CreateOAuthPinAsync("");
        await _browserService.OpenAsync(oauthUrl.Url);
        return oauthUrl;
    }
}