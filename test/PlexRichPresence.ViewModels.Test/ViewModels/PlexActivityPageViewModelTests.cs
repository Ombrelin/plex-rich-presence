using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PlexRichPresence.PlexActivity;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;
using PlexRichPresence.ViewModels.Test.Fakes;

namespace PlexRichPresence.ViewModels.Test.ViewModels;

public class PlexActivityPageViewModelTests
{
    [Fact]
    public async Task InitialStrategy_IsIdleAndDataFetchedFromStorage()
    {
        // Given
        const string fakePlexToken = "fake plex token";
        const string fakeServerIp = "111.111.111.111";
        const string fakeServerPort = "32400";
        const string fakePlexUserName = "fake plex user name";
        var storageService = new FakeStorageService(new Dictionary<string, string>
        {
            ["serverIp"] = fakeServerIp,
            ["serverPort"] = fakeServerPort,
            ["isServerOwned"] = bool.TrueString,
            ["plex_token"] = fakePlexToken,
            ["plexUserName"] = fakePlexUserName
        });
        var viewModel = new PlexActivityPageViewModel(
            new FakePlexActivityService(),
            storageService,
            new Mock<INavigationService>().Object,
            new Mock<IDiscordService>().Object,
            new Mock<ILogger<PlexActivityPageViewModel>>().Object
        );

        // When
        await viewModel.InitStrategyCommand.ExecuteAsync(null);

        // Then
        viewModel.CurrentActivity.Should().Be("Idle");
        viewModel.PlexServerIp.Should().Be(fakeServerIp);
        viewModel.PlexServerPort.Should().Be(int.Parse(fakeServerPort));
        viewModel.IsPlexServerOwned.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeServer_DisconnectsAndNavigate()
    {
        // Given
        const string fakePlexToken = "fake plex token";
        const string fakeServerIp = "111.111.111.111";
        const string fakeServerPort = "32400";
        const string fakePlexUserName = "fake plex user name";
        var storageService = new FakeStorageService(new Dictionary<string, string>
        {
            ["serverIp"] = fakeServerIp,
            ["serverPort"] = fakeServerPort,
            ["isServerOwned"] = bool.TrueString,
            ["plex_token"] = fakePlexToken,
            ["plexUserName"] = fakePlexUserName
        });

        var fakeNavigationService = new FakeNavigationService();
        var plexActivityServiceMock = new Mock<IPlexActivityService>();
        var viewModel = new PlexActivityPageViewModel(
            plexActivityServiceMock.Object,
            storageService,
            fakeNavigationService,
            new Mock<IDiscordService>().Object,
            new Mock<ILogger<PlexActivityPageViewModel>>().Object
        );
        await viewModel.InitStrategyCommand.ExecuteAsync(null);

        // When
        await viewModel.ChangeServerCommand.ExecuteAsync(null);

        // Then
        fakeNavigationService.CurrentPage.Should().Be("servers");
        plexActivityServiceMock.Verify(mock => mock.Disconnect(), Times.Once);
    }

    [Fact]
    public async Task IsOwner_GetAnOwnerStrategyAndDisplayCurrentSessionInStatus()
    {
        // Given
        const string fakePlexToken = "fake plex token";
        const string fakeServerIp = "111.111.111.111";
        const string fakeServerPort = "32400";
        const string fakePlexUserName = "fake plex user name";

        var plexActivityService = new FakePlexActivityService();
        var discordService = new FakeDiscordService();
        var storageService = new FakeStorageService(new Dictionary<string, string>
        {
            ["serverIp"] = fakeServerIp,
            ["serverPort"] = fakeServerPort,
            ["isServerOwned"] = bool.TrueString,
            ["plex_token"] = fakePlexToken,
            ["plexUserName"] = fakePlexUserName
        });
        var navigationService = new FakeNavigationService();
        var viewModel = new PlexActivityPageViewModel(
            plexActivityService,
            storageService,
            navigationService,
            discordService,
            new Mock<ILogger<PlexActivityPageViewModel>>().Object
        );
        await viewModel.InitStrategyCommand.ExecuteAsync(null);

        // When
        await viewModel.StartActivityCommand.ExecuteAsync(null);

        // Then
        viewModel.CurrentActivity.Should().Be("Test Media Title 3");
        List<string> sessions = discordService
            .Sessions
            .Select(session => session.MediaTitle)
            .ToList();
        sessions.Should().HaveCount(3);

        plexActivityService.IsOwner.Should().BeTrue();
        plexActivityService.CurrentServerIp.Should().Be(fakeServerIp);
        plexActivityService.CurrentServerPort.ToString().Should().Be(fakeServerPort);
        plexActivityService.CurrentUsername.Should().Be(fakePlexUserName);
        plexActivityService.CurrentUserToken.Should().Be(fakePlexToken);


        sessions.Should().Contain("Test Media Title 1");
        sessions.Should().Contain("Test Media Title 2");
        sessions.Should().Contain("Test Media Title 3");
    }

    [Fact]
    public async Task IsOwner_IdleEnabled_DontPostDiscordStatus()
    {
        // Given
        const string fakePlexToken = "fake plex token";
        const string fakeServerIp = "111.111.111.111";
        const string fakeServerPort = "32400";
        const string fakePlexUserName = "fake plex user name";

        var plexActivityService = new FakePlexActivityService(isIdle: true);
        var discordServiceMock = new Mock<IDiscordService>();
        var storageService = new FakeStorageService(new Dictionary<string, string>
        {
            ["serverIp"] = fakeServerIp,
            ["serverPort"] = fakeServerPort,
            ["isServerOwned"] = bool.TrueString,
            ["plex_token"] = fakePlexToken,
            ["plexUserName"] = fakePlexUserName
        });
        var navigationService = new FakeNavigationService();
        var viewModel = new PlexActivityPageViewModel(
            plexActivityService,
            storageService,
            navigationService,
            discordServiceMock.Object,
            new Mock<ILogger<PlexActivityPageViewModel>>().Object
        );
        viewModel.EnableIdleStatus = false;
        
        await viewModel.InitStrategyCommand.ExecuteAsync(null);

        // When
        await viewModel.StartActivityCommand.ExecuteAsync(null);

        // Then
        viewModel.CurrentActivity.Should().Be("Idle");
        discordServiceMock.Verify(mock => mock.SetDiscordPresenceToPlexSession(It.IsAny<IPlexSession>()), Times.Never);
        discordServiceMock.Verify(mock => mock.StopRichPresence(), Times.Exactly(3));

        plexActivityService.IsOwner.Should().BeTrue();
        plexActivityService.CurrentServerIp.Should().Be(fakeServerIp);
        plexActivityService.CurrentServerPort.ToString().Should().Be(fakeServerPort);
        plexActivityService.CurrentUsername.Should().Be(fakePlexUserName);
        plexActivityService.CurrentUserToken.Should().Be(fakePlexToken);
    }

    [Fact]
    public async Task InitStrategy_IdleEnabledInStorage_LoadsIdleEnabled()
    {
        // Given
        const string fakePlexToken = "fake plex token";
        const string fakeServerIp = "111.111.111.111";
        const string fakeServerPort = "32400";
        const string fakePlexUserName = "fake plex user name";
        var storageService = new FakeStorageService(new Dictionary<string, string>
        {
            ["serverIp"] = fakeServerIp,
            ["serverPort"] = fakeServerPort,
            ["isServerOwned"] = bool.TrueString,
            ["plex_token"] = fakePlexToken,
            ["plexUserName"] = fakePlexUserName,
            ["enableIdleStatus"] = "false",
        });
        var viewModel = new PlexActivityPageViewModel(
            new FakePlexActivityService(),
            storageService,
            new Mock<INavigationService>().Object,
            new Mock<IDiscordService>().Object,
            new Mock<ILogger<PlexActivityPageViewModel>>().Object
        );

        // When
        await viewModel.InitStrategyCommand.ExecuteAsync(null);

        // Then
        viewModel.EnableIdleStatus.Should().BeFalse();

    }

    [Fact]
    public async Task StartActivity_ActivityThrowsException_CatchAndSetsServerUnreachable()
    {
        // Given
        const string fakePlexToken = "fake plex token";
        const string fakeServerIp = "111.111.111.111";
        const string fakeServerPort = "32400";
        const string fakePlexUserName = "fake plex user name";

        Mock<IPlexActivityService> plexActivityServiceMock = new Mock<IPlexActivityService>();
        plexActivityServiceMock.Setup(mock => mock.GetSessions(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<string>()))
            .Throws(() => new Exception());
        var discordService = new FakeDiscordService();
        var storageService = new FakeStorageService(new Dictionary<string, string>
        {
            ["serverIp"] = fakeServerIp,
            ["serverPort"] = fakeServerPort,
            ["isServerOwned"] = bool.TrueString,
            ["plex_token"] = fakePlexToken,
            ["plexUserName"] = fakePlexUserName
        });
        var navigationService = new FakeNavigationService();
        var viewModel = new PlexActivityPageViewModel(
            plexActivityServiceMock.Object,
            storageService,
            navigationService,
            discordService,
            new Mock<ILogger<PlexActivityPageViewModel>>().Object
        );
        await viewModel.InitStrategyCommand.ExecuteAsync(null);

        // When
        await viewModel.StartActivityCommand.ExecuteAsync(null);

        // Then
        viewModel.IsServerUnreachable.Should().BeTrue();
    }
}