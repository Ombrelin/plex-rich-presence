using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Media;
using PlexRichPresence.ViewModels.Services;
using Websocket.Client;

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