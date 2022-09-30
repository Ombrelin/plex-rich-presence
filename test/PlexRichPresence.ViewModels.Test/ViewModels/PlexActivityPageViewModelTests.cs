using FluentAssertions;
using Moq;
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
            new Mock<IDiscordService>().Object
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
    public async Task ChangeServer_Navigate()
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
        var viewModel = new PlexActivityPageViewModel(
            new FakePlexActivityService(), 
            storageService,
            fakeNavigationService,
            new Mock<IDiscordService>().Object
        );
        await viewModel.InitStrategyCommand.ExecuteAsync(null);
        
        // When
        await viewModel.ChangeServerCommand.ExecuteAsync(null);

        // Then
        fakeNavigationService.CurrentPage.Should().Be("servers");
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
            discordService
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
}