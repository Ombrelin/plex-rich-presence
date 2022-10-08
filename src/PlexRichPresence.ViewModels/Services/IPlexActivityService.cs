using PlexRichPresence.ViewModels.Models;

namespace PlexRichPresence.PlexActivity;

public interface IPlexActivityService
{
    IAsyncEnumerable<IPlexSession> GetSessions(bool isOwner, string userId, string serverIp, int serverPort, string userToken);
}