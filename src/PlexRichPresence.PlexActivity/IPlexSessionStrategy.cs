using PlexRichPresence.Core;

namespace PlexRichPresence.PlexActivity;

public interface IPlexSessionStrategy
{
    IAsyncEnumerable<PlexSession> GetSessions(string username, string serverIp, int serverPort, string userToken);
    void Disconnect();
}