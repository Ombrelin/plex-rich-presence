using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Plex.ServerApi.Clients.Interfaces;
using Xunit;

namespace PlexRichPresence.PlexActivity.Tests;

public class PlexActivityServiceTests
{
    [Fact]
    public void GetSessions_IsOwner_ReturnsPollingStrategy()
    {
        // Given
        var plexActivityService =
            new PlexActivityService(new Mock<IPlexServerClient>().Object, new Mock<ILogger>().Object);

        // When
        IPlexSessionStrategy strategy = plexActivityService.GetStrategy(true);

        // Then
        strategy.Should().BeOfType<PlexSessionsPollingStrategy>();
    }

    [Fact]
    public void GetSessions_IsNotOwner_ReturnsWebSocketStrategy()
    {
        // Given
        var plexActivityService =
            new PlexActivityService(new Mock<IPlexServerClient>().Object, new Mock<ILogger>().Object);

        // When
        IPlexSessionStrategy strategy = plexActivityService.GetStrategy(false);

        // Then
        strategy.Should().BeOfType<PlexSessionsWebSocketStrategy>();
    }
}