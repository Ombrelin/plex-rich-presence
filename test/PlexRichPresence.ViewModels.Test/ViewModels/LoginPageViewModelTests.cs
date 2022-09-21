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
    public async Task FillUsernameAndPasswordThenClickConnect_CredentialValid_StoresTokenAndIdAndThenNavigates()
    {
        // Given
        const string fakeUsername = "username";
        const string fakePassword = "password";
        const string fakeUserToken = "token";
        const string fakePlexUserId = "plex user id";

        Mock<IPlexAccountClient> plexAccountClientMock = new Mock<IPlexAccountClient>();
        plexAccountClientMock
            .Setup(mock => mock.GetPlexAccountAsync(fakeUsername, fakePassword))
            .Returns(() => Task.FromResult(new PlexAccount { AuthToken = fakeUserToken, Uuid = fakePlexUserId }));
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
        (await storageService.GetAsync("plexUserId")).Should().Be(fakePlexUserId);
        browserService.OpenedUrls.Should().BeEmpty();
    }

    [Fact]
    public async Task ConnectViaSso_Success_StoresTokenAndIdAndThenNavigates()
    {
        // Given
        const string fakeUserToken = "token";
        const string fakeOauthUrl = "plex oauth url";
        const int fakeOauthPinId = 999;
        const string fakePlexUserId = "plex user id";
        int index = 0;
        Mock<IPlexAccountClient> plexAccountClientMock = new Mock<IPlexAccountClient>();
        plexAccountClientMock
            .Setup(mock => mock.CreateOAuthPinAsync(It.IsAny<string>()))
            .Returns(() => Task.FromResult(new OAuthPin { Id = fakeOauthPinId, Url = fakeOauthUrl }));
        plexAccountClientMock
            .Setup(mock => mock.GetAuthTokenFromOAuthPinAsync(fakeOauthPinId.ToString()))
            .Returns(() =>
            {
                if (index < 3)
                {
                    ++index;
                    return Task.FromResult(new OAuthPin());
                }

                return Task.FromResult(new OAuthPin { AuthToken = fakeUserToken });
            });
        plexAccountClientMock
            .Setup(mock => mock.GetPlexAccountAsync(fakeUserToken))
            .Returns(() => Task.FromResult(new PlexAccount { AuthToken = fakeUserToken, Uuid = fakePlexUserId }));

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
        (await storageService.GetAsync("plexUserId")).Should().Be(fakePlexUserId);
        browserService.OpenedUrls.Should().Contain(fakeOauthUrl);
    }

    [Theory]
    [InlineData(null,null, false)]
    [InlineData("","", false)]
    [InlineData("test","", false)]
    [InlineData("test",null, false)]
    [InlineData("","test", false)]
    [InlineData(null,"test", false)]
    [InlineData("test","test", true)]
    public void CanLoginWithCredentials(string login, string password, bool expected)
    {
        // Given
        var viewModel = new LoginPageViewModel(
            null,
            null,
            null,
            null
        );
        
        // When
        viewModel.Login = login;
        viewModel.Password = password;
        
        // Then
        viewModel.CanLoginWithCredentials.Should().Be(expected);
    }
}