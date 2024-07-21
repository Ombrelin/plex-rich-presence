using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.PlexActivity;

public class PlexSessionsPollingStrategy : IPlexSessionStrategy
{
    private bool _isDisconnected;

    private readonly IClock _clock;
    private readonly ILogger<PlexSessionsPollingStrategy> _logger;
    private readonly IPlexServerClient _plexServerClient;
    private readonly PlexSessionMapper _plexSessionMapper;

    public PlexSessionsPollingStrategy(ILogger<PlexSessionsPollingStrategy> logger, IPlexServerClient plexServerClient,
        IClock clock, PlexSessionMapper plexSessionMapper)
    {
        _logger = logger;
        _plexServerClient = plexServerClient;
        _clock = clock;
        _plexSessionMapper = plexSessionMapper;
    }
    
    public async IAsyncEnumerable<PlexSession> GetSessions(string username, string serverIp, int serverPort, string userToken)
    {
        _logger.LogInformation("Listening to sessions via polling for user: {Username}", username);
        while (!_isDisconnected)
        {
            var plexServerHost = new Uri($"http://{serverIp}:{serverPort}").ToString();
            var sessions = await _plexServerClient.GetSessionsAsync(userToken, plexServerHost);
            
            await _clock.Delay(TimeSpan.FromSeconds(2));

            if (sessions.Metadata is null)
            {
                _logger.LogInformation("No session: Idling");
                yield return new PlexSession();
                continue;
            }

            var currentUserSession = sessions.Metadata.FirstOrDefault(session => session.User.Title == username);
            if (currentUserSession is null)
            {
                _logger.LogInformation("No session: Idling");
                yield return new PlexSession();
                continue;
            }

            var plexSession = _plexSessionMapper.Map(currentUserSession, plexServerHost, userToken);
            _logger.LogInformation("Found session {Session}", plexSession.MediaParentTitle);
            yield return plexSession;
        }
    }

    public void Disconnect()
    {
        _logger.LogInformation("Disconnected");
        _isDisconnected = true;
    }
}