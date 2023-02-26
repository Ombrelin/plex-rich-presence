using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.PlexActivity;

public class PlexActivityService : IPlexActivityService
{
    private readonly IPlexServerClient plexServerClient;
    private readonly ILogger<PlexSessionsWebSocketStrategy> wsLogger;
    private readonly ILogger<PlexSessionsPollingStrategy> pollingLogger;
    private readonly IClock clock;
    private readonly PlexSessionMapper plexSessionMapper;
    private IPlexSessionStrategy? strategy;

    public PlexActivityService(IPlexServerClient plexServerClient, IClock clock,
        ILogger<PlexSessionsWebSocketStrategy> wsLogger, ILogger<PlexSessionsPollingStrategy> pollingLogger,
        PlexSessionMapper plexSessionMapper)
    {
        this.plexServerClient = plexServerClient;
        this.plexServerClient = plexServerClient;
        this.clock = clock;
        this.wsLogger = wsLogger;
        this.pollingLogger = pollingLogger;
        this.plexSessionMapper = plexSessionMapper;
    }


    public IAsyncEnumerable<PlexSession> GetSessions(bool isOwner, string userId, string serverIp, int serverPort,
        string userToken)
    {
        this.strategy = isOwner
            ? new PlexSessionsPollingStrategy(pollingLogger, plexServerClient, this.clock, plexSessionMapper)
            : new PlexSessionsWebSocketStrategy(wsLogger, plexServerClient, new WebSocketClientFactory(),
                plexSessionMapper);

        return strategy.GetSessions(
            userId,
            serverIp,
            serverPort,
            userToken);
    }

    public void Disconnect()
    {
        this.strategy?.Disconnect();
    }
}