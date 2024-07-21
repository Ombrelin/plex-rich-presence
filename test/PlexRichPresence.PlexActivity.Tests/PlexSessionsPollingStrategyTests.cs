using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    [Fact]
    public async Task GetSessions_GetsSessionsEvery2Sec()
    {
        // Given
        const int elementsCountForTest = 3;
        const string fakeToken = "test plex token";
        const string fakeServerIp = "111.111.111.111";
        const int fakeServerPort = 32400;
        const string fakeUserName = "fake user name";
        
        var serverClientMock = SetupPlexClientServerClientMock(fakeToken, fakeServerIp, fakeServerPort, fakeUserName);
        var clock = new FakeClock(DateTime.Now);
        var now = DateTime.Now;

        var strategy = new PlexSessionsPollingStrategy(new Mock<ILogger<PlexSessionsPollingStrategy>>().Object, serverClientMock.Object, clock, new PlexSessionMapper());
        var result = new List<PlexSession>();

        // When
        var sessionsStream = strategy.GetSessions(fakeUserName, fakeServerIp, fakeServerPort, fakeToken);
        await foreach (var plexSession in sessionsStream)
        {
            result.Add(plexSession);
            if (result.Count == elementsCountForTest)
                strategy.Disconnect();
        }

        // Then
        result.Should().HaveCount(elementsCountForTest);
        var titles = result.Select(session => session.MediaTitle).ToList();

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
        const string fakeToken = "test plex token";
        const string fakeServerIp = "111.111.111.111";
        const int fakeServerPort = 32400;
        const string fakeUserName = "fake user name";
        
        var serverClientMock = SetupPlexClientServerClientMockNoAlwaysSession(fakeToken, fakeServerIp, fakeServerPort, fakeUserName);
        var clock = new FakeClock(DateTime.Now);
        var now = DateTime.Now;
        
        var strategy = new PlexSessionsPollingStrategy(new Mock<ILogger<PlexSessionsPollingStrategy>>().Object, serverClientMock.Object, clock, new PlexSessionMapper());
        var result = new List<PlexSession>();

        // When
        var sessionsStream = strategy.GetSessions(fakeUserName, fakeServerIp, fakeServerPort, fakeToken);
        await foreach (var plexSession in sessionsStream)
        {
            result.Add(plexSession);
            if (result.Count == elementsCountForTest)
                strategy.Disconnect();
        }

        // Then
        result.Should().HaveCount(elementsCountForTest);
        var titles = result.Select(session => session.MediaTitle).ToList();

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
        const string fakeToken = "test plex token";
        const string fakeServerIp = "111.111.111.111";
        const int fakeServerPort = 32400;
        const string fakeUserName = "fake user name";
        var serverClientMock = SetupPlexClientServerClientNoAlwaysSessionForUserMock(fakeToken, fakeServerIp, fakeServerPort, fakeUserName);

        var clock = new FakeClock(DateTime.Now);
        var now = DateTime.Now;
        var strategy = new PlexSessionsPollingStrategy(new Mock<ILogger<PlexSessionsPollingStrategy>>().Object, serverClientMock.Object, clock, new PlexSessionMapper());

        var result = new List<PlexSession>();
        
        // When
        var sessionsStream = strategy.GetSessions(fakeUserName, fakeServerIp, fakeServerPort, fakeToken);
        await foreach (var plexSession in sessionsStream)
        {
            result.Add(plexSession);
            if (result.Count == elementsCountForTest)
                strategy.Disconnect();
        }

        // Then
        result.Should().HaveCount(elementsCountForTest);
        var titles = result.Select(session => session.MediaTitle).ToList();

        titles[0].Should().Contain("Idle");
        titles[1].Should().Contain("Test Media 2");
        titles[2].Should().Contain("Test Media 3");

        clock.DateTimeAfterDelay.Should().BeCloseTo(now.AddSeconds(6), TimeSpan.FromMilliseconds(10));
    }


    private static Mock<IPlexServerClient> SetupPlexClientServerClientMock(string fakeToken, string fakeServerIp, int fakeServerPort, string fakeUserName)
    {
        var mediaCount = 0;

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

    private static Mock<IPlexServerClient> SetupPlexClientServerClientNoAlwaysSessionForUserMock(string fakeToken, string fakeServerIp, int fakeServerPort, string fakeUserName)
    {
        var mediaCount = 0;

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

    private static Mock<IPlexServerClient> SetupPlexClientServerClientMockNoAlwaysSession(string fakeToken, string fakeServerIp, int fakeServerPort, string fakeUserName)
    {
        var mediaCount = 0;

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
}