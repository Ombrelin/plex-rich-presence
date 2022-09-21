using FluentAssertions;
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
        const string fakePlexUserId = "fake plex user id";
        var storageService = new FakeStorageService(new Dictionary<string, string>
        {
            ["serverIp"] = fakeServerIp,
            ["serverPort"] = fakeServerPort,
            ["isServerOwned"] = bool.TrueString,
            ["plex_token"] = fakePlexToken,
            ["plexUserId"] = fakePlexUserId
        });
        var viewModel = new PlexActivityPageViewModel(new FakePlexActivityService(), storageService, null);

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
        const string fakePlexUserId = "fake plex user id";
        var storageService = new FakeStorageService(new Dictionary<string, string>
        {
            ["serverIp"] = fakeServerIp,
            ["serverPort"] = fakeServerPort,
            ["isServerOwned"] = bool.TrueString,
            ["plex_token"] = fakePlexToken,
            ["plexUserId"] = fakePlexUserId
        });

        var fakeNavigationService = new FakeNavigationService();
        var viewModel = new PlexActivityPageViewModel(new FakePlexActivityService(), storageService, fakeNavigationService);
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
        const string fakePlexUserId = "fake plex user id";

        var plexActivityService = new FakePlexActivityService();
        var storageService = new FakeStorageService(new Dictionary<string, string>
        {
            ["serverIp"] = fakeServerIp,
            ["serverPort"] = fakeServerPort,
            ["isServerOwned"] = bool.TrueString,
            ["plex_token"] = fakePlexToken,
            ["plexUserId"] = fakePlexUserId
        });
        var navigationService = new FakeNavigationService();
        var viewModel = new PlexActivityPageViewModel(
            plexActivityService,
            storageService,
            navigationService
        );
        await viewModel.InitStrategyCommand.ExecuteAsync(null);

        // When

        await viewModel.StartActivityCommand.ExecuteAsync(null);

        // Then
        viewModel.CurrentActivity.Should().Be("Test Media Title 3");
    }
}