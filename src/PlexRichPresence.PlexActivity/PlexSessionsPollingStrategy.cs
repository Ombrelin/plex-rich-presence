using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Server.Sessions;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.PlexActivity;

public class PlexSessionsPollingStrategy : IPlexSessionStrategy
{
    private bool isDisconnected = false;

    private readonly IClock clock;
    private readonly ILogger logger;
    private readonly IPlexServerClient plexServerClient;

    public PlexSessionsPollingStrategy(ILogger logger, IPlexServerClient plexServerClient, IClock clock)
    {
        this.logger = logger;
        this.plexServerClient = plexServerClient;
        this.clock = clock;
    }


    public async IAsyncEnumerable<PlexSession> GetSessions(string userId, string serverIp, int serverPort,
        string userToken)
    {
        logger.LogInformation("Listening to sessions via polling for {ServerIp}", serverIp);
        while (!isDisconnected)
        {
            SessionContainer sessions = await plexServerClient.GetSessionsAsync(
                userToken,
                new Uri($"http://{serverIp}:{serverPort}").ToString()
            );
            await this.clock.Delay(TimeSpan.FromSeconds(2));

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
            
            yield return new PlexSession(currentUserSession);
        }
    }

    public void Disconnect()
    {
        this.isDisconnected = true;
    }
}