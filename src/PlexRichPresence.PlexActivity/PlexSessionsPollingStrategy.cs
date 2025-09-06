using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Server.Sessions;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.PlexActivity;

public class PlexSessionsPollingStrategy : IPlexSessionStrategy
{
    private bool isDisconnected = false;

    private readonly IClock clock;
    private readonly ILogger<PlexSessionsPollingStrategy> logger;
    private readonly IPlexServerClient plexServerClient;
    private readonly PlexSessionMapper plexSessionMapper;

    public PlexSessionsPollingStrategy(ILogger<PlexSessionsPollingStrategy> logger, IPlexServerClient plexServerClient,
        IClock clock, PlexSessionMapper plexSessionMapper)
    {
        this.logger = logger;
        this.plexServerClient = plexServerClient;
        this.clock = clock;
        this.plexSessionMapper = plexSessionMapper;
    }


    public async IAsyncEnumerable<PlexSession> GetSessions(string username, string serverIp, int serverPort,
        string userToken)
    {
        logger.LogInformation("Listening to sessions via polling for user : {Username}", username);
        while (!isDisconnected)
        {
            var plexServerHost = new Uri($"http://{serverIp}:{serverPort}").ToString();
            SessionContainer sessions = await plexServerClient.GetSessionsAsync(
                userToken,
                plexServerHost
            );
            await this.clock.Delay(TimeSpan.FromSeconds(2));

            if (sessions.Metadata is null)
            {
                this.logger.LogInformation("No session : Idling");
                yield return new PlexSession();
                continue;
            }

            var currentUserSessions = sessions
                .Metadata
                .Where(s => s.User.Title == username)
                .Select(s => plexSessionMapper.Map(s, plexServerHost, userToken));

            PlexSession? plexSession = SelectActiveSessionFromUserSessions(currentUserSessions);

            if (plexSession == null)
            {
                this.logger.LogInformation("No session : Idling");
                yield return new PlexSession();
                continue;
            }

            this.logger.LogInformation("Found session {Session}", plexSession.MediaParentTitle);
            yield return plexSession;
        }
    }

    private static PlexSession? SelectActiveSessionFromUserSessions(IEnumerable<PlexSession> currentUserSessions)
    {
        return currentUserSessions.FirstOrDefault(s => s.PlayerState == PlexPlayerState.Playing) ??
               currentUserSessions.FirstOrDefault(s => s.PlayerState == PlexPlayerState.Buffering) ??
               currentUserSessions.FirstOrDefault(s => s.PlayerState == PlexPlayerState.Paused) ??
               currentUserSessions.FirstOrDefault(s => s.PlayerState == PlexPlayerState.Idle);
    }

    public void Disconnect()
    {
        this.logger.LogInformation("Disconnected");
        this.isDisconnected = true;
    }
}