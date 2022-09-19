using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.PlexActivity;

public class PlexActivityService : IPlexActivityService
{
    private readonly IPlexServerClient plexServerClient;
    private readonly ILogger logger;
    private IPlexSessionStrategy strategy;

    public PlexActivityService(IPlexServerClient plexServerClient, ILogger logger)
    {
        this.plexServerClient = plexServerClient;
        this.plexServerClient = plexServerClient;
        this.logger = logger;
    }


    public IPlexSessionStrategy GetStrategy(bool isOwner)
    {
        return isOwner
            ? new PlexSessionsPollingStrategy(logger, plexServerClient)
            : new PlexSessionsWebSocketStrategy(logger, plexServerClient, new WebSocketClientFactory());
    }
}