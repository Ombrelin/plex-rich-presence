using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.PlexActivity;

public interface IPlexSessionStrategy
{
    IAsyncEnumerable<PlexSession> GetSessions(string userId, string serverIp, int serverPort, string userToken);
    void Disconnect();
}