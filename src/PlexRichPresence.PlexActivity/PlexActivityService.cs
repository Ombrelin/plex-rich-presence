using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.PlexActivity;

public class PlexActivityService : IPlexActivityService
{
    private readonly IPlexServerClient plexServerClient;
    private readonly ILogger<PlexSessionsWebSocketStrategy> wsLogger;
    private readonly ILogger<PlexSessionsPollingStrategy> pollingLogger;
    private readonly IClock clock;
    private IPlexSessionStrategy? strategy;

    public PlexActivityService(IPlexServerClient plexServerClient, IClock clock,
        ILogger<PlexSessionsWebSocketStrategy> wsLogger, ILogger<PlexSessionsPollingStrategy> pollingLogger)
    {
        this.plexServerClient = plexServerClient;
        this.plexServerClient = plexServerClient;
        this.clock = clock;
        this.wsLogger = wsLogger;
        this.pollingLogger = pollingLogger;
    }


    public IAsyncEnumerable<IPlexSession> GetSessions(bool isOwner, string userId, string serverIp, int serverPort,
        string userToken)
    {
        this.strategy = isOwner
            ? new PlexSessionsPollingStrategy(pollingLogger, plexServerClient, this.clock)
            : new PlexSessionsWebSocketStrategy(wsLogger, plexServerClient, new WebSocketClientFactory());

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