using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Server.Sessions;
using PlexRichPresence.Core;
using PlexRichPresence.Tests.Common;
using Xunit;

namespace PlexRichPresence.PlexActivity.Tests;

public class PlexSessionsPollingStrategyTests
{
    const string fakeToken = "test plex token";
    const string fakeServerIp = "111.111.111.111";
    const int fakeServerPort = 32400;
    const string fakeUserName = "fake user name";

    [Fact]
    public async Task GetSessions_GetsSessionsEvery2Sec()
    {
        // Given
        const int elementsCountForTest = 3;
        Mock<IPlexServerClient> serverClientMock = SetupPlexClientServerClientMock();

        var clock = new FakeClock(DateTime.Now);
        DateTime now = DateTime.Now;

        var strategy = new PlexSessionsPollingStrategy(
            new Mock<ILogger<PlexSessionsPollingStrategy>>().Object,
            serverClientMock.Object,
            clock,
            new PlexSessionMapper()
        );

        var result = new List<PlexSession>();

        // When
        var sessionsStream = strategy.GetSessions(
            fakeUserName,
            fakeServerIp,
            fakeServerPort,
            fakeToken
        );

        await foreach (PlexSession plexSession in sessionsStream)
        {
            result.Add(plexSession);
            if (result.Count == elementsCountForTest)
            {
                strategy.Disconnect();
            }
        }

        // Then
        result
            .Should()
            .HaveCount(elementsCountForTest);

        List<string> titles = result
            .Select(session => session.MediaTitle)
            .ToList();

        titles.Should().Contain("Test Media 1");
        titles.Should().Contain("Test Media 2");
        titles.Should().Contain("Test Media 3");


        clock.DateTimeAfterDelay.Should().BeCloseTo(now.AddSeconds(6), TimeSpan.FromMilliseconds(10));
    }

    [Fact]
    public async Task GetSessions_NoSession_YieldIdleSession()
    {
        // Given
        const int elementsCountForTest = 3;
        Mock<IPlexServerClient> serverClientMock = SetupPlexClientServerClientMockNoAlwaysSession();

        var clock = new FakeClock(DateTime.Now);
        DateTime now = DateTime.Now;
        var strategy = new PlexSessionsPollingStrategy(
            new Mock<ILogger<PlexSessionsPollingStrategy>>().Object,
            serverClientMock.Object,
            clock,
            new PlexSessionMapper()
        );

        var result = new List<PlexSession>();

        // When
        IAsyncEnumerable<PlexSession> sessionsStream = strategy.GetSessions(
            fakeUserName,
            fakeServerIp,
            fakeServerPort,
            fakeToken
        );
        await foreach (PlexSession plexSession in sessionsStream)
        {
            result.Add(plexSession);
            if (result.Count == elementsCountForTest)
            {
                strategy.Disconnect();
            }
        }

        // Then
        result
            .Should()
            .HaveCount(elementsCountForTest);

        List<string> titles = result
            .Select(session => session.MediaTitle)
            .ToList();

        titles[0].Should().Contain("Idle");
        titles[1].Should().Contain("Test Media 2");
        titles[2].Should().Contain("Test Media 3");

        clock.DateTimeAfterDelay.Should().BeCloseTo(now.AddSeconds(6), TimeSpan.FromMilliseconds(10));
    }


    [Fact]
    public async Task GetSessions_NoSessionForUser_YieldIdleSessions()
    {
        // Given
        const int elementsCountForTest = 3;
        var serverClientMock = SetupPlexClientServerClientWhenSessionFromTargetUserAndAnotherUser();

        var clock = new FakeClock(DateTime.Now);
        DateTime now = DateTime.Now;
        var strategy = new PlexSessionsPollingStrategy(
            new Mock<ILogger<PlexSessionsPollingStrategy>>().Object,
            serverClientMock.Object,
            clock,
            new PlexSessionMapper()
        );

        var result = new List<PlexSession>();


        // When
        IAsyncEnumerable<PlexSession> sessionsStream = strategy.GetSessions(
            fakeUserName,
            fakeServerIp,
            fakeServerPort,
            fakeToken
        );
        await foreach (PlexSession plexSession in sessionsStream)
        {
            result.Add(plexSession);
            if (result.Count == elementsCountForTest)
            {
                strategy.Disconnect();
            }
        }

        // Then
        result
            .Should()
            .HaveCount(elementsCountForTest);

        List<string> titles = result
            .Select(session => session.MediaTitle)
            .ToList();

        titles[0].Should().Contain("Idle");
        titles[1].Should().Contain("Test Media 2");
        titles[2].Should().Contain("Test Media 3");


        clock.DateTimeAfterDelay.Should().BeCloseTo(now.AddSeconds(6), TimeSpan.FromMilliseconds(10));
    }

    [Fact]
    public async Task GetSessions_WhenMultipleSessionsForUser_PicksPlayingSessions()
    {
        // Given
        const int elementsCountForTest = 1;
        var sessions = new List<SessionMetadata>
        {
            new()
            {
                Title = "Test Media 1",
                Player = new Player
                {
                    State = "buffering"
                },
                User = new User
                {
                    Title = fakeUserName
                }
            },
            new()
            {
                Title = "Test Media 2",
                Player = new Player
                {
                    State = "paused"
                },
                User = new User
                {
                    Title = fakeUserName
                }
            },
            new()
            {
                Title = "Test Media 3",
                Player = new Player() { State = "playing" },
                User = new User
                {
                    Title = fakeUserName
                }
            }
        };
        var serverClientMock = SetupPlexClientServerClientMockWhenSessionsList(sessions);
        var strategy = new PlexSessionsPollingStrategy(
            new Mock<ILogger<PlexSessionsPollingStrategy>>().Object,
            serverClientMock.Object,
            new FakeClock(DateTime.Now),
            new PlexSessionMapper()
        );

        var result = new List<PlexSession>();

        // When
        IAsyncEnumerable<PlexSession> sessionsStream = strategy.GetSessions(
            fakeUserName,
            fakeServerIp,
            fakeServerPort,
            fakeToken
        );
        await foreach (PlexSession plexSession in sessionsStream)
        {
            result.Add(plexSession);
            if (result.Count == elementsCountForTest)
            {
                strategy.Disconnect();
            }
        }

        // Then
        result
            .Should()
            .HaveCount(elementsCountForTest);

        List<string> titles = result
            .Select(session => session.MediaTitle)
            .ToList();

        titles.First().Should().Contain("Test Media 3");
    }
    
    [Fact]
    public async Task GetSessions_WhenMultipleSessionsForUserAndNoPLaying_PicksBufferingSessions()
    {
        // Given
        const int elementsCountForTest = 1;
        var sessions = new List<SessionMetadata>
        {
            
            new()
            {
                Title = $"Test Media 2",
                Player = new Player
                {
                    State = "paused"
                },
                User = new User
                {
                    Title = fakeUserName
                }
            },
            new()
            {
                Title = $"Test Media 1",
                Player = new Player
                {
                    State = "buffering"
                },
                User = new User
                {
                    Title = fakeUserName
                }
            }
        };
        var serverClientMock = SetupPlexClientServerClientMockWhenSessionsList(sessions);
        var strategy = new PlexSessionsPollingStrategy(
            new Mock<ILogger<PlexSessionsPollingStrategy>>().Object,
            serverClientMock.Object,
            new FakeClock(DateTime.Now),
            new PlexSessionMapper()
        );

        var result = new List<PlexSession>();

        // When
        IAsyncEnumerable<PlexSession> sessionsStream = strategy.GetSessions(
            fakeUserName,
            fakeServerIp,
            fakeServerPort,
            fakeToken
        );
        await foreach (PlexSession plexSession in sessionsStream)
        {
            result.Add(plexSession);
            if (result.Count == elementsCountForTest)
            {
                strategy.Disconnect();
            }
        }

        // Then
        result
            .Should()
            .HaveCount(elementsCountForTest);

        List<string> titles = result
            .Select(session => session.MediaTitle)
            .ToList();

        titles.First().Should().Contain("Test Media 1");
    }

    private static Mock<IPlexServerClient> SetupPlexClientServerClientMock()
    {
        int mediaCount = 0;

        var serverClientMock = new Mock<IPlexServerClient>();
        serverClientMock
            .Setup(mock => mock.GetSessionsAsync(
                fakeToken,
                new Uri($"http://{fakeServerIp}:{fakeServerPort}").ToString()
            ))
            .Returns(() => Task.FromResult(new SessionContainer
            {
                Metadata = new List<SessionMetadata>
                {
                    new()
                    {
                        Title = $"Test Media {++mediaCount}",
                        User = new User
                        {
                            Title = fakeUserName
                        }
                    }
                }
            }));
        return serverClientMock;
    }

    private static Mock<IPlexServerClient> SetupPlexClientServerClientWhenSessionFromTargetUserAndAnotherUser()
    {
        int mediaCount = 0;

        var serverClientMock = new Mock<IPlexServerClient>();
        serverClientMock
            .Setup(mock => mock.GetSessionsAsync(
                fakeToken,
                new Uri($"http://{fakeServerIp}:{fakeServerPort}").ToString()
            ))
            .Returns(() =>
            {
                if (mediaCount > 0)
                {
                    return Task.FromResult(new SessionContainer
                    {
                        Metadata = new List<SessionMetadata>
                        {
                            new()
                            {
                                Title = $"Test Media {++mediaCount}",
                                User = new User
                                {
                                    Title = fakeUserName
                                }
                            }
                        }
                    });
                }

                ++mediaCount;
                return Task.FromResult(new SessionContainer
                {
                    Metadata = new List<SessionMetadata>
                    {
                        new()
                        {
                            Title = $"Test Media {mediaCount}",
                            User = new User
                            {
                                Title = "something else"
                            }
                        }
                    }
                });
            });
        return serverClientMock;
    }

    private static Mock<IPlexServerClient> SetupPlexClientServerClientMockNoAlwaysSession()
    {
        int mediaCount = 0;

        var serverClientMock = new Mock<IPlexServerClient>();
        serverClientMock
            .Setup(mock => mock.GetSessionsAsync(
                fakeToken,
                new Uri($"http://{fakeServerIp}:{fakeServerPort}").ToString()
            ))
            .Returns(() =>
            {
                if (mediaCount > 0)
                {
                    return Task.FromResult(new SessionContainer
                    {
                        Metadata = new List<SessionMetadata>
                        {
                            new()
                            {
                                Title = $"Test Media {++mediaCount}",
                                User = new User
                                {
                                    Title = fakeUserName
                                }
                            }
                        }
                    });
                }

                ++mediaCount;
                return Task.FromResult(new SessionContainer());
            });
        return serverClientMock;
    }

    private static Mock<IPlexServerClient> SetupPlexClientServerClientMockWhenSessionsList(
        List<SessionMetadata> sessions)
    {
        var serverClientMock = new Mock<IPlexServerClient>();
        serverClientMock
            .Setup(mock => mock.GetSessionsAsync(
                fakeToken,
                new Uri($"http://{fakeServerIp}:{fakeServerPort}").ToString()
            ))
            .Returns(() => Task.FromResult(new SessionContainer
            {
                Metadata = sessions
            }));
        return serverClientMock;
    }
}