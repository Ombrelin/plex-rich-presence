using FluentAssertions;
using Moq;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Account;
using Plex.ServerApi.PlexModels.OAuth;
using PlexRichPresence.Tests.Common;
using PlexRichPresence.ViewModels.Services;
using PlexRichPresence.ViewModels.Test.Fakes;

namespace PlexRichPresence.ViewModels.Test.ViewModels;

public class LoginPageViewModelTests
{
    [Fact]
    public async Task FillUsernameAndPasswordThenClickConnect_CredentialValid_StoresTokenAndIdAndThenNavigates()
    {
        // Given
        const string fakeLogin = "username";
        const string fakePassword = "password";
        const string fakeUserToken = "token";
        const string fakePlexUserName = "plex user name";

        Mock<IPlexAccountClient> plexAccountClientMock = new();
        plexAccountClientMock.Setup(mock => mock.GetPlexAccountAsync(fakeLogin, fakePassword)).Returns(() => Task.FromResult(new PlexAccount { AuthToken = fakeUserToken, Username = fakePlexUserName }));
        var navigationService = new FakeNavigationService();
        var storageService = new FakeStorageService();
        var browserService = new FakeBrowserService();

        var viewModel = new LoginPageViewModel(plexAccountClientMock.Object, navigationService, storageService, browserService, new Mock<IClock>().Object);

        // When
        viewModel.Login = fakeLogin;
        viewModel.Password = fakePassword;
        await viewModel.LoginWithCredentialsCommand.ExecuteAsync(null);

        // Then
        navigationService.CurrentPage.Should().Be("servers");
        (await storageService.GetAsync("plex_token")).Should().Be(fakeUserToken);
        (await storageService.GetAsync("plexUserName")).Should().Be(fakePlexUserName);
        browserService.OpenedUrls.Should().BeEmpty();
    }

    [Fact]
    public async Task ConnectViaSso_Success_StoresTokenAndIdAndThenNavigates()
    {
        // Given
        const string fakeUserToken = "token";
        const string fakeOauthUrl = "plex oauth url";
        const int fakeOauthPinId = 999;
        const string fakePlexUserName = "plex user name";
        var index = 0;
        var plexAccountClientMock = new Mock<IPlexAccountClient>();
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
            .Returns(() => Task.FromResult(new PlexAccount { AuthToken = fakeUserToken, Username = fakePlexUserName }));

        var navigationService = new FakeNavigationService();
        var storageService = new FakeStorageService();
        var browserService = new FakeBrowserService();
        var now = DateTime.Now;
        var clock = new FakeClock(now);

        var viewModel = new LoginPageViewModel(
            plexAccountClientMock.Object,
            navigationService,
            storageService,
            browserService,
            clock
        );

        // When
        await viewModel.LoginWithBrowserCommand.ExecuteAsync(null);

        // Then
        navigationService.CurrentPage.Should().Be("servers");
        (await storageService.GetAsync("plex_token")).Should().Be(fakeUserToken);
        (await storageService.GetAsync("plexUserName")).Should().Be(fakePlexUserName);
        browserService.OpenedUrls.Should().Contain(fakeOauthUrl);
        clock.DateTimeAfterDelay
            .Should()
            .BeCloseTo(now.Add(TimeSpan.FromSeconds(6)), TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null, null, false)]
    [InlineData("", "", false)]
    [InlineData("test", "", false)]
    [InlineData("test", null, false)]
    [InlineData("", "test", false)]
    [InlineData(null, "test", false)]
    [InlineData("test", "test", true)]
    public void CanLoginWithCredentials(string login, string password, bool expected)
    {
        // Given
        var viewModel = new LoginPageViewModel(new Mock<IPlexAccountClient>().Object, new Mock<INavigationService>().Object, new Mock<IStorageService>().Object, new Mock<IBrowserService>().Object, new Mock<IClock>().Object);

        // When
        viewModel.Login = login;
        viewModel.Password = password;

        // Then
        viewModel.CanLoginWithCredentials.Should().Be(expected);
    }
}