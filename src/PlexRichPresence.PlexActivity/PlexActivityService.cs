using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.PlexActivity;

public class PlexActivityService : IPlexActivityService
{
    private readonly IPlexServerClient _plexServerClient;
    private readonly ILogger<PlexSessionsWebSocketStrategy> _wsLogger;
    private readonly ILogger<PlexSessionsPollingStrategy> _pollingLogger;
    private readonly IClock _clock;
    private readonly PlexSessionMapper _plexSessionMapper;
    private IPlexSessionStrategy? _strategy;

    public PlexActivityService(IPlexServerClient plexServerClient, IClock clock,
        ILogger<PlexSessionsWebSocketStrategy> wsLogger, ILogger<PlexSessionsPollingStrategy> pollingLogger,
        PlexSessionMapper plexSessionMapper)
    {
        _plexServerClient = plexServerClient;
        _plexServerClient = plexServerClient;
        _clock = clock;
        _wsLogger = wsLogger;
        _pollingLogger = pollingLogger;
        _plexSessionMapper = plexSessionMapper;
    }


    public IAsyncEnumerable<PlexSession> GetSessions(bool isOwner, string userId, string serverIp, int serverPort, string userToken)
    {
        _strategy = isOwner
            ? new PlexSessionsPollingStrategy(_pollingLogger, _plexServerClient, _clock, _plexSessionMapper)
            : new PlexSessionsWebSocketStrategy(_wsLogger, _plexServerClient, new WebSocketClientFactory(), _plexSessionMapper);

        return _strategy.GetSessions(userId, serverIp, serverPort, userToken);
    }

    public void Disconnect()
    {
        _strategy?.Disconnect();
    }
}