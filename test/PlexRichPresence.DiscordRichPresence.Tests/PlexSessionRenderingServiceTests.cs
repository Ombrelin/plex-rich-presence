using DiscordRPC;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PlexRichPresence.Core;
using PlexRichPresence.DiscordRichPresence.Rendering;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Tests;

public class PlexSessionRenderingServiceTests
{
    public static TheoryData<PlexSession, ActivityType, StatusDisplayType, string?, string?, bool, string?>
        RenderingTheoryData =>
        new()
        {
            // Movie
            {
                new PlexSession
                {
                    MediaTitle = "Test Movie",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = "Thumbnail URL",
                    MediaType = PlexMediaType.Movie, 
                    PlayerState = PlexPlayerState.Buffering,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Watching,
                StatusDisplayType.Details,
                null, // Status
                "Test Movie", // Details
                false, // Expect Timestamp
                "sand-clock"
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Movie",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = "Thumbnail URL",
                    MediaType = PlexMediaType.Movie,
                    PlayerState = PlexPlayerState.Paused,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Watching,
                StatusDisplayType.Details,
                null, // Status
                "Test Movie", // Details
                false, // Expect Timestamp
                "pause-circle"
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Movie",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = "Thumbnail URL",
                    MediaType = PlexMediaType.Movie,
                    PlayerState = PlexPlayerState.Playing,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Watching,
                StatusDisplayType.Details,
                null, // Status
                "Test Movie", // Details
                true, // Expect Timestamp
                null
            },

            // Unknown
            {
                new PlexSession
                {
                    MediaTitle = "Test Unknown",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = null,
                    MediaType = PlexMediaType.Unknown,
                    PlayerState = PlexPlayerState.Buffering,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Watching,
                StatusDisplayType.Name,
                "Test Unknown", // Status
                "Test Grand Parent Title - Test Parent Title", // Details
                false, // Expect Timestamp
                "sand-clock"
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Unknown",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = null,
                    MediaType = PlexMediaType.Unknown,
                    PlayerState = PlexPlayerState.Paused,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Watching,
                StatusDisplayType.Name,
                "Test Unknown", // Status
                "Test Grand Parent Title - Test Parent Title", // Details
                false, // Expect Timestamp
                "pause-circle"
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Unknown",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = null,
                    MediaType = PlexMediaType.Unknown,
                    PlayerState = PlexPlayerState.Playing,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Watching,
                StatusDisplayType.Name,
                "Test Unknown", // Status
                "Test Grand Parent Title - Test Parent Title", // Details
                true, // Expect Timestamp
                null
            },
            
            // Track
            {
                new PlexSession
                {
                    MediaTitle = "Test Track",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = "Thumbnail URL",
                    MediaType = PlexMediaType.Track,
                    PlayerState = PlexPlayerState.Buffering,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Listening,
                StatusDisplayType.State,
                "Test Grand Parent Title", // Status
                "Test Track", // Details
                false, // Expect Timestamp
                "sand-clock"
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Track",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = "Thumbnail URL",
                    MediaType = PlexMediaType.Track,
                    PlayerState = PlexPlayerState.Paused,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Listening,
                StatusDisplayType.State,
                "Test Grand Parent Title", // Status
                "Test Track", // Details
                false, // Expect Timestamp
                "pause-circle"
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Track",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = "Thumbnail URL",
                    MediaType = PlexMediaType.Track,
                    PlayerState = PlexPlayerState.Playing,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Listening,
                StatusDisplayType.State,
                "Test Grand Parent Title", // Status
                "Test Track", // Details
                true, // Expect Timestamp
                null
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Track",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = "Thumbnail URL",
                    MediaType = PlexMediaType.Track,
                    PlayerState = PlexPlayerState.Playing,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Listening,
                StatusDisplayType.State,
                "Test Grand Parent Title", // Status
                "Test Track", // Details
                true, // Expect Timestamp
                null
            },
            // Episode
            {
                new PlexSession
                {
                    MediaTitle = "Test Episode",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = "Thumbnail URL",
                    MediaType = PlexMediaType.Episode,
                    PlayerState = PlexPlayerState.Buffering,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Watching,
                StatusDisplayType.Details,
                "Test Grand Parent Title", // Status
                "Test Episode", // Details
                false, // Expect Timestamp
                "sand-clock"
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Episode",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = "Thumbnail URL",
                    MediaType = PlexMediaType.Episode,
                    PlayerState = PlexPlayerState.Paused,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Watching,
                StatusDisplayType.Details,
                "Test Grand Parent Title", // Status
                "Test Episode", // Details
                false, // Expect Timestamp
                "pause-circle"
            },
            {
                new PlexSession
                {
                    MediaTitle = "Test Episode",
                    MediaParentTitle = "Test Parent Title",
                    MediaGrandParentTitle = "Test Grand Parent Title",
                    Thumbnail = "Thumbnail URL",
                    MediaType = PlexMediaType.Episode,
                    PlayerState = PlexPlayerState.Playing,
                    Duration = 20_000,
                    ViewOffset = 10_000,
                },
                ActivityType.Watching,
                StatusDisplayType.Details,
                "Test Grand Parent Title", // Status
                "Test Episode", // Details
                true, // Expect Timestamp
                null
            },

            // Idle
            {
                new PlexSession
                {
                    MediaTitle = "Idle",
                    MediaParentTitle = string.Empty,
                    MediaGrandParentTitle = string.Empty,
                    Thumbnail = null,
                    MediaType = PlexMediaType.Idle,
                    PlayerState = PlexPlayerState.Idle
                },
                ActivityType.Playing,
                StatusDisplayType.Name,
                "Idle", // Status
                null, // Details
                false, // Expect Timestamp
                "sleep-mode"
            }
        };


    [Theory]
    [MemberData(nameof(RenderingTheoryData))]
    public void RenderPlayerState(
        PlexSession session,
        ActivityType expectedActivityType,
        StatusDisplayType expectedStatusType,
        string? expectedState,
        string? expectedDetail,
        bool expectTimestampPresent,
        string? playerStatusAsset
    )
    {
        // Given
        DateTime dateTime = DateTime.Now.ToUniversalTime();
        Mock<IClock> mockClock = SharedSetup.BuildMockClock(dateTime);
        var plexSessionRenderingService =
            new PlexSessionRenderingService(new PlexSessionRendererFactory(mockClock.Object),
                new Mock<ILogger<PlexSessionRenderingService>>().Object);

        // When
        RichPresence presence = plexSessionRenderingService.RenderSession(session);

        // Then
        presence.Type.Should().Be(expectedActivityType);
        presence.StatusDisplay.Should().Be(expectedStatusType);
        presence.State.Should().Be(expectedState);
        presence.Details.Should().Be(expectedDetail);
        if (expectTimestampPresent)
        {
            presence.Timestamps.Start.Should().Be(dateTime.AddSeconds(session.ViewOffset / 1000 * -1));
            presence.Timestamps.End.Should().Be(dateTime.AddSeconds((session.Duration - session.ViewOffset) / 1000));
        }

        presence.Assets.SmallImageKey.Should().Be(playerStatusAsset);
        presence.Assets.LargeImageKey.Should().Be(session.Thumbnail);
    }
}