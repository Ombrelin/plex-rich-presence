using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Account;
using PlexRichPresence.ViewModels.Services;
using PlexRichPresence.ViewModels.Test.Fakes;

namespace PlexRichPresence.ViewModels.Test.ViewModels;

public class ServersPageViewModelTests
{
    [Fact]
    public async Task GetData_PopulatesUI()
    {
        // Given
        const string fakePlexToken = "fake plex token";
        const string fakeUsername = "username";
        const string fakeThumbnail = "thumbnail";
        const string fakeServerName = "Test Server";
        const string fakeServerIp = "111.111.111.111";
        const string fakeServerPort = "32400";

        var fakeStorageService = new FakeStorageService();
        await fakeStorageService.PutAsync("plex_token", fakePlexToken);
        var plexAccountClientMock =
            BuildPlexAccountClientMock(fakePlexToken, fakeUsername, fakeThumbnail, fakeServerName, fakeServerIp,
                fakeServerPort);
        var navigationService = new FakeNavigationService();

        var viewModel = new ServersPageViewModel(
            plexAccountClientMock.Object,
            fakeStorageService,
            navigationService,
            Mock.Of<ILogger<PlexActivityPageViewModel>>()
        );

        // When
        await viewModel.GetDataCommand.ExecuteAsync(null);

        // Then
        viewModel.Username.Should().Be(fakeUsername);
        viewModel.ThumbnailUrl.Should().Be(fakeThumbnail);
        viewModel.Servers.Should().HaveCount(1);
        viewModel.Servers.First().Name.Should().Be(fakeServerName);
    }

    [Fact]
    public void CanValidate_NoServerSelected_False()
    {
        // Given
        var viewModel = new ServersPageViewModel(
            Mock.Of<IPlexAccountClient>(),
            Mock.Of<IStorageService>(),
            Mock.Of<INavigationService>(),
            Mock.Of<ILogger<PlexActivityPageViewModel>>()
        );

        // Then
        viewModel.CanValidate.Should().BeFalse();
    }

    [Fact]
    public void CanValidate_ServerSelected_True()
    {
        // Given
        var viewModel = new ServersPageViewModel(
            Mock.Of<IPlexAccountClient>(),
            Mock.Of<IStorageService>(),
            Mock.Of<INavigationService>(),
            Mock.Of<ILogger<PlexActivityPageViewModel>>()
        );

        // When
        viewModel.SelectedServer = new AccountServer();

        // Then
        viewModel.CanValidate.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "", false)]
    [InlineData(null, null, false)]
    [InlineData("", "32400", false)]
    [InlineData(null, "32400", false)]
    [InlineData("111.111.111.111", "", false)]
    [InlineData("111.111.111.111", null, false)]
    [InlineData("111.111.111.111", "32400", true)]
    public void CanValidate_CustomServer(string ip, string port, bool expected)
    {
        // Given
        var viewModel = new ServersPageViewModel(
            Mock.Of<IPlexAccountClient>(),
            Mock.Of<IStorageService>(),
            Mock.Of<INavigationService>(),
            Mock.Of<ILogger<PlexActivityPageViewModel>>()
        );
        viewModel.UseCustomServer = true;

        // When
        viewModel.CustomServerIp = ip;
        viewModel.CustomServerPort = port;

        // Then
        viewModel.CanValidate.Should().Be(expected);
    }

    [Fact]
    public async Task ValidateWithExistingServer_SetServerInfoInStorage()
    {
        // Given
        const string fakePlexToken = "fake plex token";
        const string fakeUsername = "username";
        const string fakeThumbnail = "thumbnail";
        const string fakeServerName = "Test Server";
        const string fakeServerIp = "111.111.111.111";
        const string fakeServerPort = "32400";

        var fakeStorageService = new FakeStorageService();
        await fakeStorageService.PutAsync("plex_token", fakePlexToken);
        var plexAccountClientMock =
            BuildPlexAccountClientMock(fakePlexToken, fakeUsername, fakeThumbnail, fakeServerName, fakeServerIp,
                fakeServerPort);
        var navigationService = new FakeNavigationService();

        var viewModel = new ServersPageViewModel(
            plexAccountClientMock.Object,
            fakeStorageService,
            navigationService,
            Mock.Of<ILogger<PlexActivityPageViewModel>>()
        );

        await viewModel.GetDataCommand.ExecuteAsync(null);

        // When
        viewModel.SelectedServer = viewModel.Servers.First();
        await viewModel.ValidateServerSelectionCommand.ExecuteAsync(null);

        // Then
        viewModel.SelectedServer.Name.Should().Be(fakeServerName);
        (await fakeStorageService.GetAsync("serverIp")).Should().Be(fakeServerIp);
        (await fakeStorageService.GetAsync("serverPort")).Should().Be(fakeServerPort);
        (await fakeStorageService.GetAsync("isServerOwned")).Should().Be(bool.TrueString);
        navigationService.CurrentPage.Should().Be("activity");
    }

    [Fact]
    public async Task ValidateWithCustom_SetServerInfoInStorage()
    {
        // Given
        const string fakePlexToken = "fake plex token";
        const string fakeUsername = "username";
        const string fakeThumbnail = "thumbnail";
        const string fakeServerName = "Test Server";
        const string fakeServerIp = "111.111.111.111";
        const string fakeServerPort = "32400";

        var fakeStorageService = new FakeStorageService();
        await fakeStorageService.PutAsync("plex_token", fakePlexToken);
        var plexAccountClientMock =
            BuildPlexAccountClientMock(fakePlexToken, fakeUsername, fakeThumbnail, fakeServerName, fakeServerIp,
                fakeServerPort);
        var navigationService = new FakeNavigationService();

        var viewModel = new ServersPageViewModel(
            plexAccountClientMock.Object,
            fakeStorageService,
            navigationService,
            Mock.Of<ILogger<PlexActivityPageViewModel>>()
        );

        await viewModel.GetDataCommand.ExecuteAsync(null);

        // When
        viewModel.UseCustomServer = true;
        viewModel.CustomServerIp = fakeServerIp;
        viewModel.CustomServerPort = fakeServerPort;
        viewModel.IsCustomServerOwned = true;
        await viewModel.ValidateServerSelectionCommand.ExecuteAsync(null);

        // Then
        (await fakeStorageService.GetAsync("serverIp")).Should().Be(fakeServerIp);
        (await fakeStorageService.GetAsync("serverPort")).Should().Be(fakeServerPort);
        (await fakeStorageService.GetAsync("isServerOwned")).Should().Be(bool.TrueString);
        navigationService.CurrentPage.Should().Be("activity");
    }


    [Fact]
    public async Task GetData_InvalidToken_NavigateToLogin()
    {
        // Given
        const string fakePlexToken = "fake plex token";
        const string fakeUsername = "username";
        const string fakeThumbnail = "thumbnail";
        const string fakeServerName = "Test Server";
        const string fakeServerIp = "111.111.111.111";
        const string fakeServerPort = "32400";

        var fakeStorageService = new FakeStorageService();
        await fakeStorageService.PutAsync("plex_token", fakePlexToken);
        var plexAccountClientMock = new Mock<IPlexAccountClient>();
        plexAccountClientMock.Setup(mock => mock.GetPlexAccountAsync(It.IsAny<string>()))
            .ThrowsAsync(new ApplicationException("Unsuccessful response from 3rd Party API"));
        plexAccountClientMock.Setup(mock => mock.GetAccountServersAsync(It.IsAny<string>()))
            .ThrowsAsync(new ApplicationException("Unsuccessful response from 3rd Party API"));
        var navigationService = new FakeNavigationService();

        var viewModel = new ServersPageViewModel(
            plexAccountClientMock.Object,
            fakeStorageService,
            navigationService,
            Mock.Of<ILogger<PlexActivityPageViewModel>>()
        );

        // When
        await viewModel.GetDataCommand.ExecuteAsync(null);

        // Then
        navigationService.CurrentPage.Should().Be("login");
    }

    private static Mock<IPlexAccountClient> BuildPlexAccountClientMock(
        string fakePlexToken,
        string fakeUsername,
        string fakeThumbnail,
        string fakeServerName, string fakeServerIp, string fakeServerPort)
    {
        Mock<IPlexAccountClient> plexAccountClientMock = new();
        plexAccountClientMock.Setup(mock => mock.GetAccountServersAsync(fakePlexToken))
            .Returns(() => Task.FromResult(new AccountServerContainer
            {
                Servers = new List<AccountServer>
                {
                    new()
                    {
                        Name = fakeServerName,
                        Address = fakeServerIp,
                        Port = int.Parse(fakeServerPort),
                        Owned = 1
                    }
                }
            }));
        plexAccountClientMock.Setup(mock => mock.GetPlexAccountAsync(fakePlexToken))
            .Returns(() => Task.FromResult(new PlexAccount
            {
                Username = fakeUsername,
                Thumb = fakeThumbnail
            }));
        return plexAccountClientMock;
    }
}