using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Server.Sessions;
using PlexRichPresence.Core;
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
                .Where(s => s.User.Title == username).Select(s => plexSessionMapper.Map(s, plexServerHost, userToken));

            if (!currentUserSessions.Any())
            {
                this.logger.LogInformation("No session : Idling");
                yield return new PlexSession();
                continue;
            }

            var plexSession = currentUserSessions.FirstOrDefault(s => s.PlayerState == ViewModels.Models.PlexPlayerState.Playing) ??
                              currentUserSessions.FirstOrDefault(s => s.PlayerState == ViewModels.Models.PlexPlayerState.Buffering) ??
                              currentUserSessions.FirstOrDefault(s => s.PlayerState == ViewModels.Models.PlexPlayerState.Paused) ??
                              currentUserSessions.FirstOrDefault(s => s.PlayerState == ViewModels.Models.PlexPlayerState.Idle) ??
                              new PlexSession(); // This shouldn't *really* happen since we check before, but I don't like the nullable warning so its here

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