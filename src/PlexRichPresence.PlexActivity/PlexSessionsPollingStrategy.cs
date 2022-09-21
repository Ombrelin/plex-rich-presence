using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Server.Sessions;

namespace PlexRichPresence.PlexActivity;

public class PlexSessionsPollingStrategy : IPlexSessionStrategy
{
    private bool isDisconnected = false;

    private readonly ILogger logger;
    private readonly IPlexServerClient plexServerClient;

    public PlexSessionsPollingStrategy(ILogger logger, IPlexServerClient plexServerClient)
    {
        this.logger = logger;
        this.plexServerClient = plexServerClient;
    }


    public async IAsyncEnumerable<PlexSession> GetSessions(string userId, string serverIp, int serverPort,
        string userToken)
    {
        while (!isDisconnected)
        {
            SessionContainer sessions = await plexServerClient.GetSessionsAsync(
                userToken,
                new Uri($"http://{serverIp}:{serverPort}").ToString()
            );
            await Task.Delay(TimeSpan.FromSeconds(2));

            if (sessions.Metadata is null)
            {
                continue;
            }
            
            SessionMetadata? currentUserSession = sessions
                .Metadata
                .FirstOrDefault(session => session.User.Id == userId);

            if (currentUserSession is null)
            {
                continue;
            }
            
            yield return new PlexSession(Title: currentUserSession.Title);
        }
    }

    public void Disconnect()
    {
        this.isDisconnected = true;
    }
}