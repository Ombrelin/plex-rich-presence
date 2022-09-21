using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Server.Sessions;
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
        const string fakeUserId = "fake user id";
        var serverClientMock = SetupPlexClientServerClientMock(
            fakeToken,
            fakeServerIp,
            fakeServerPort,
            fakeUserId);

        var strategy = new PlexSessionsPollingStrategy(
            new Mock<ILogger>().Object,
            serverClientMock.Object
        );

        var result = new List<PlexSession>();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // When
        var sessionsStream = strategy.GetSessions(
            fakeUserId,
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

        stopwatch.Stop();

        // Then
        result
            .Should()
            .HaveCount(elementsCountForTest);

        var titles = result
            .Select(session => session.Title)
            .ToList();

        titles.Should().Contain("Test Media 1");
        titles.Should().Contain("Test Media 2");
        titles.Should().Contain("Test Media 3");

        stopwatch
            .Elapsed
            .Should()
            .BeGreaterThan(TimeSpan.FromSeconds(6));
    }

    [Fact]
    public async Task GetSessions_NoSession_DontYieldSession()
    {
        // Given
        const int elementsCountForTest = 3;
        const string fakeToken = "test plex token";
        const string fakeServerIp = "111.111.111.111";
        const int fakeServerPort = 32400;
        const string fakeUserId = "fake user id";
        var serverClientMock = SetupPlexClientServerClientMockNoAlwaysSession(
            fakeToken,
            fakeServerIp,
            fakeServerPort,
            fakeUserId
        );

        var strategy = new PlexSessionsPollingStrategy(
            new Mock<ILogger>().Object,
            serverClientMock.Object
        );

        var result = new List<PlexSession>();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // When
        IAsyncEnumerable<PlexSession> sessionsStream = strategy.GetSessions(
            fakeUserId,
            fakeServerIp,
            fakeServerPort,
            fakeToken
        );
        await foreach (PlexSession plexSession in sessionsStream)
        {
            result.Add(plexSession);
            if (result.Count == elementsCountForTest - 1)
            {
                strategy.Disconnect();
            }
        }

        stopwatch.Stop();

        // Then
        result
            .Should()
            .HaveCount(elementsCountForTest - 1);

        var titles = result
            .Select(session => session.Title)
            .ToList();

        titles.Should().Contain("Test Media 2");
        titles.Should().Contain("Test Media 3");

        stopwatch
            .Elapsed
            .Should()
            .BeGreaterThan(TimeSpan.FromSeconds(6));
    }


    [Fact]
    public async Task GetSessions_NoSessionForUser_DontYieldSession()
    {
        // Given
        const int elementsCountForTest = 3;
        const string fakeToken = "test plex token";
        const string fakeServerIp = "111.111.111.111";
        const int fakeServerPort = 32400;
        const string fakeUserId = "fake user id";
        var serverClientMock = SetupPlexClientServerClientNoAlwaysSessionForUserMock(
            fakeToken,
            fakeServerIp,
            fakeServerPort,
            fakeUserId
        );

        var strategy = new PlexSessionsPollingStrategy(
            new Mock<ILogger>().Object,
            serverClientMock.Object
        );

        var result = new List<PlexSession>();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // When
        IAsyncEnumerable<PlexSession> sessionsStream = strategy.GetSessions(
            fakeUserId,
            fakeServerIp,
            fakeServerPort,
            fakeToken
        );
        await foreach (PlexSession plexSession in sessionsStream)
        {
            result.Add(plexSession);
            if (result.Count == elementsCountForTest - 1)
            {
                strategy.Disconnect();
            }
        }

        stopwatch.Stop();

        // Then
        result
            .Should()
            .HaveCount(elementsCountForTest - 1);

        var titles = result
            .Select(session => session.Title)
            .ToList();

        titles.Should().Contain("Test Media 2");
        titles.Should().Contain("Test Media 3");

        stopwatch
            .Elapsed
            .Should()
            .BeGreaterThan(TimeSpan.FromSeconds(6));
    }


    private static Mock<IPlexServerClient> SetupPlexClientServerClientMock(
        string fakeToken,
        string fakeServerIp,
        int fakeServerPort,
        string fakeUserId
    )
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
                            Id = fakeUserId
                        }
                    }
                }
            }));
        return serverClientMock;
    }

    private static Mock<IPlexServerClient> SetupPlexClientServerClientNoAlwaysSessionForUserMock(
        string fakeToken,
        string fakeServerIp,
        int fakeServerPort,
        string fakeUserId
    )
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
                                    Id = fakeUserId
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
                                Id = "something else"
                            }
                        }
                    }
                });
            });
        return serverClientMock;
    }

    private static Mock<IPlexServerClient> SetupPlexClientServerClientMockNoAlwaysSession(
        string fakeToken,
        string fakeServerIp,
        int fakeServerPort,
        string fakeUserId
    )
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
                                    Id = fakeUserId
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