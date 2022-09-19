using FluentAssertions;
using Moq;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Account;
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
            BuildPlexAccountClientMock(fakePlexToken, fakeUsername, fakeThumbnail, fakeServerName, fakeServerIp, fakeServerPort);
        var navigationService = new FakeNavigationService();

        var viewModel = new ServersPageViewModel(
            plexAccountClientMock.Object,
            fakeStorageService,
            navigationService
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
            BuildPlexAccountClientMock(fakePlexToken, fakeUsername, fakeThumbnail, fakeServerName, fakeServerIp, fakeServerPort);
        var navigationService = new FakeNavigationService();

        var viewModel = new ServersPageViewModel(
            plexAccountClientMock.Object,
            fakeStorageService,
            navigationService
        );
        
        await viewModel.GetDataCommand.ExecuteAsync(null);
        
        // When
        viewModel.SelectedServer = viewModel.Servers.First();
        await viewModel.ValidateServerSelectionCommand.ExecuteAsync(null);
        
        // Then
        viewModel.SelectedServer.Name.Should().Be(fakeServerName);
        (await fakeStorageService.GetAsync("serverIp")).Should().Be(fakeServerIp);
        (await fakeStorageService.GetAsync("serverPort")).Should().Be(fakeServerPort);
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
            BuildPlexAccountClientMock(fakePlexToken, fakeUsername, fakeThumbnail, fakeServerName, fakeServerIp, fakeServerPort);
        var navigationService = new FakeNavigationService();

        var viewModel = new ServersPageViewModel(
            plexAccountClientMock.Object,
            fakeStorageService,
            navigationService
        );
        
        await viewModel.GetDataCommand.ExecuteAsync(null);
        
        // When
        viewModel.UseCustomServer = true;
        viewModel.CustomServerIp = fakeServerIp;
        viewModel.CustomServerPort = fakeServerPort;
        await viewModel.ValidateServerSelectionCommand.ExecuteAsync(null);
        
        // Then
        (await fakeStorageService.GetAsync("serverIp")).Should().Be(fakeServerIp);
        (await fakeStorageService.GetAsync("serverPort")).Should().Be(fakeServerPort);
        navigationService.CurrentPage.Should().Be("activity");
    }

    private static Mock<IPlexAccountClient> BuildPlexAccountClientMock(
        string fakePlexToken,
        string fakeUsername,
        string fakeThumbnail,
        string fakeServerName, string fakeServerIp, string fakeServerPort)
    {
        Mock<IPlexAccountClient> plexAccountClientMock = new Mock<IPlexAccountClient>();
        plexAccountClientMock.Setup(mock => mock.GetAccountServersAsync(fakePlexToken))
            .Returns(() => Task.FromResult(new AccountServerContainer
            {
                Servers = new List<AccountServer>
                {
                    new AccountServer
                    {
                        Name = fakeServerName,
                        Address = fakeServerIp,
                        Port = int.Parse(fakeServerPort)
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