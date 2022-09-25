using DiscordRPC;
using FluentAssertions;
using Moq;
using PlexRichPresence.DiscordRichPresence.Rendering;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Tests;

public class PlexSessionRenderingServiceTests
{
    public static TheoryData<FakePlexSession, string, string, TimeSpan>
        RenderingTheoryData =>
        new()
        {
            {
                new FakePlexSession
                {
                    MediaTitle = "Test Movie", MediaType = PlexMediaType.Movie, PlayerState = PlexPlayerState.Buffering
                },
                "⟲", "Test Movie", TimeSpan.Zero
            },
            {
                new FakePlexSession
                {
                    MediaTitle = "Test Movie", MediaType = PlexMediaType.Movie, PlayerState = PlexPlayerState.Paused
                },
                "⏸", "Test Movie", TimeSpan.Zero
            },
            {
                new FakePlexSession
                {
                    MediaTitle = "Test Movie", MediaType = PlexMediaType.Movie, PlayerState = PlexPlayerState.Playing,
                    Duration = 20_000, ViewOffset = 10_000
                },
                "▶", "Test Movie", TimeSpan.FromSeconds(10)
            },
            {
                new FakePlexSession
                {
                    MediaTitle = "Test Title", MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title", MediaType = PlexMediaType.Unknown,
                    PlayerState = PlexPlayerState.Paused
                },
                "Test Title", "Test Grand Parent Title - Test Parent Title", TimeSpan.Zero
            },
            {
                new FakePlexSession
                {
                    MediaTitle = "Test Title", MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title", MediaType = PlexMediaType.Track,
                    PlayerState = PlexPlayerState.Paused
                },
                "⏸ Test Grand Parent Title", "♫ Test Title", TimeSpan.Zero
            }
            ,
            {
                new FakePlexSession
                {
                    MediaTitle = "Test Title", MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title", MediaType = PlexMediaType.Episode,
                    PlayerState = PlexPlayerState.Paused
                },
                "⏸ Test Grand Parent Title", "⏏ Test Title", TimeSpan.Zero
            }
        };


    [Theory]
    [MemberData(nameof(RenderingTheoryData))]
    public void RenderPlayerState(
        FakePlexSession session,
        string expectedState,
        string expectedDetail,
        TimeSpan expectedEndTimestampAfterNow
    )
    {
        // Given
        DateTime dateTime = DateTime.Now;
        var mockClock = SharedSetup.BuildMockClock(dateTime);
        var plexSessionRenderingService =
            new PlexSessionRenderingService(new PlexSessionRendererFactory(mockClock.Object));
        
        // When
        RichPresence presence = plexSessionRenderingService.RenderSession(session);

        // Then
        presence.State.Should().Be(expectedState);
        presence.Details.Should().Be(expectedDetail);
        presence.Timestamps.End.Should().Be(dateTime.Add(expectedEndTimestampAfterNow));
    }
}