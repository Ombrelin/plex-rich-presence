using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.PlexActivity;

public class PlexActivityService : IPlexActivityService
{
    private readonly IPlexServerClient plexServerClient;
    private readonly ILogger logger;
    private readonly IClock clock;

    public PlexActivityService(IPlexServerClient plexServerClient, ILogger logger, IClock clock)
    {
        this.plexServerClient = plexServerClient;
        this.plexServerClient = plexServerClient;
        this.logger = logger;
        this.clock = clock;
    }
    

    public IAsyncEnumerable<IPlexSession> GetSessions(bool isOwner, string userId, string serverIp, int serverPort, string userToken)
    {
        IPlexSessionStrategy strategy = isOwner
            ? new PlexSessionsPollingStrategy(logger, plexServerClient, this.clock)
            : new PlexSessionsWebSocketStrategy(logger, plexServerClient, new WebSocketClientFactory());

        return strategy.GetSessions(
            userId,
            serverIp,
            serverPort,
            userToken);
    }
}