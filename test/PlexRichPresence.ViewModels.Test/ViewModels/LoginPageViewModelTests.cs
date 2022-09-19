using FluentAssertions;
using Moq;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Account;
using Plex.ServerApi.PlexModels.OAuth;
using PlexRichPresence.ViewModels.Test.Fakes;

namespace PlexRichPresence.ViewModels.Test.ViewModels;

public class LoginPageViewModelTests
{
    [Fact]
    public async Task FillUsernameAndPasswordThenClickConnect_CredentialValid_StoresTokenAndNavigates()
    {
        // Given
        const string fakeUsername = "username";
        const string fakePassword = "password";
        const string fakeUserToken = "token";

        Mock<IPlexAccountClient> plexAccountClientMock = new Mock<IPlexAccountClient>();
        plexAccountClientMock
            .Setup(mock => mock.GetPlexAccountAsync(fakeUsername, fakePassword))
            .Returns(() => Task.FromResult(new PlexAccount { AuthToken = fakeUserToken }));
        var navigationService = new FakeNavigationService();
        var storageService = new FakeStorageService();
        var browserService = new FakeBrowserService();

        var viewModel = new LoginPageViewModel(
            plexAccountClientMock.Object,
            navigationService,
            storageService,
            browserService
        );

        // When
        viewModel.Login = fakeUsername;
        viewModel.Password = fakePassword;
        await viewModel.LoginWithCredentialsCommand.ExecuteAsync(null);

        // Then
        navigationService.CurrentPage.Should().Be("servers");
        (await storageService.GetAsync("plex_token")).Should().Be(fakeUserToken);
        browserService.OpenedUrls.Should().BeEmpty();
    }

    [Fact]
    public async Task ConnectViaSso_Success_StoresTokenAndNavigates()
    {
        // Given
        const string fakeUsername = "username";
        const string fakePassword = "password";
        const string fakeUserToken = "token";
        const string fakeOauthUrl = "plex oauth url";
        const int fakeOauthPinId = 999;

        Mock<IPlexAccountClient> plexAccountClientMock = new Mock<IPlexAccountClient>();
        plexAccountClientMock
            .Setup(mock => mock.CreateOAuthPinAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(new OAuthPin { Id = fakeOauthPinId, Url = "fakeOauthUrl" }));
        plexAccountClientMock
            .Setup(mock => mock.GetAuthTokenFromOAuthPinAsync(fakeOauthPinId.ToString()))
            .Returns(() => Task.FromResult(new OAuthPin { AuthToken = fakeUserToken }));

        var navigationService = new FakeNavigationService();
        var storageService = new FakeStorageService();
        var browserService = new FakeBrowserService();

        var viewModel = new LoginPageViewModel(
            plexAccountClientMock.Object,
            navigationService,
            storageService,
            browserService
        );

        // When
        await viewModel.LoginWithBrowserCommand.ExecuteAsync(null);

        // Then
        navigationService.CurrentPage.Should().Be("servers");
        (await storageService.GetAsync("plex_token")).Should().Be(fakeUserToken);
        browserService.OpenedUrls.Should().Contain("fakeOauthUrl");
    }
}