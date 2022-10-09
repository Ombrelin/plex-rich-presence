using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Server.Sessions;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.PlexActivity;

public class PlexSessionsPollingStrategy : IPlexSessionStrategy
{
    private bool isDisconnected = false;

    private readonly IClock clock;
    private readonly ILogger<PlexSessionsPollingStrategy> logger;
    private readonly IPlexServerClient plexServerClient;

    public PlexSessionsPollingStrategy(ILogger<PlexSessionsPollingStrategy> logger, IPlexServerClient plexServerClient, IClock clock)
    {
        this.logger = logger;
        this.plexServerClient = plexServerClient;
        this.clock = clock;
    }


    public async IAsyncEnumerable<PlexSession> GetSessions(string username, string serverIp, int serverPort,
        string userToken)
    {
        logger.LogInformation("Listening to sessions via polling for IP : {ServerIp} for User : {Username}", serverIp, username);
        while (!isDisconnected)
        {
            SessionContainer sessions = await plexServerClient.GetSessionsAsync(
                userToken,
                new Uri($"http://{serverIp}:{serverPort}").ToString()
            );
            await this.clock.Delay(TimeSpan.FromSeconds(2));

            if (sessions.Metadata is null)
            {
                this.logger.LogInformation("No session : Idling");
                yield return new PlexSession();
                continue;
            }
            
            SessionMetadata? currentUserSession = sessions
                .Metadata
                .FirstOrDefault(session => session.User.Title == username);

            if (currentUserSession is null)
            {
                this.logger.LogInformation("No session : Idling");
                yield return new PlexSession();
                continue;
            }
            
            var plexSession = new PlexSession(currentUserSession);
            this.logger.LogInformation("Found session {Session}", plexSession.MediaParentTitle);
            yield return plexSession;
        }
    }

    public void Disconnect()
    {
        this.logger.LogInformation("Disconnected");
        this.isDisconnected = true;
    }
}