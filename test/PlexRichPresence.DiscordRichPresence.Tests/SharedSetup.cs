using Moq;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Tests;

public class SharedSetup
{
    public static Mock<IClock> BuildMockClock(DateTime now)
    {
        var mockClock = new Mock<IClock>();
        mockClock.Setup(mock => mock.Now).Returns(() => now);
        return mockClock;
    }
}