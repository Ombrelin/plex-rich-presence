using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PlexRichPresence.Core;
using PlexRichPresence.DiscordRichPresence.Rendering;

namespace PlexRichPresence.DiscordRichPresence.Tests;

public class PlexSessionRenderingServiceTests
{
    public static TheoryData<PlexSession, string?, string?, bool, TimeSpan>
        RenderingTheoryData =>
        new()
        {
            {
                new PlexSession
                {
                    MediaTitle = "Test Movie", MediaType = PlexMediaType.Movie, PlayerState = PlexPlayerState.Buffering
                },
                "⟲\x2800", "Test Movie", true, TimeSpan.Zero
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Movie", MediaType = PlexMediaType.Movie, PlayerState = PlexPlayerState.Paused
                },
                "⏸\x2800", "Test Movie", true, TimeSpan.Zero
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Movie", MediaType = PlexMediaType.Movie, PlayerState = PlexPlayerState.Playing,
                    Duration = 20_000, ViewOffset = 10_000
                },
                "▶\x2800", "Test Movie", true, TimeSpan.FromSeconds(10)
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Title", MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title", MediaType = PlexMediaType.Unknown,
                    PlayerState = PlexPlayerState.Paused
                },
                "Test Title", "Test Grand Parent Title - Test Parent Title", true, TimeSpan.Zero
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Title", MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title", MediaType = PlexMediaType.Track,
                    PlayerState = PlexPlayerState.Paused
                },
                "⏸ Test Grand Parent Title", "♫ Test Title", true, TimeSpan.Zero
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Title", MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title", MediaType = PlexMediaType.Episode,
                    PlayerState = PlexPlayerState.Paused
                },
                "⏸ Test Grand Parent Title", "⏏ Test Title", true, TimeSpan.Zero
            },
            {
                new PlexSession
                {
                    MediaTitle = "Idle",
                    MediaParentTitle = string.Empty,
                    MediaGrandParentTitle = string.Empty,
                    MediaType = PlexMediaType.Idle,
                    PlayerState = PlexPlayerState.Idle
                },
                "Idle", null, false, TimeSpan.Zero
            }
        };


    [Theory]
    [MemberData(nameof(RenderingTheoryData))]
    public void RenderPlayerState(PlexSession session, string? expectedState, string? expectedDetail, bool setEndTimeStamp, TimeSpan expectedEndTimestampAfterNow)
    {
        // Given
        var dateTime = DateTime.Now.ToUniversalTime();
        var mockClock = SharedSetup.BuildMockClock(dateTime);
        var plexSessionRenderingService =
            new PlexSessionRenderingService(new PlexSessionRendererFactory(mockClock.Object),
                new Mock<ILogger<PlexSessionRenderingService>>().Object);

        // When
        var presence = plexSessionRenderingService.RenderSession(session);

        // Then
        presence.State.Should().Be(expectedState);
        presence.Details.Should().Be(expectedDetail);
        
        if (setEndTimeStamp)
            presence.Timestamps.End.Should().Be(dateTime.Add(expectedEndTimestampAfterNow));
    }
}