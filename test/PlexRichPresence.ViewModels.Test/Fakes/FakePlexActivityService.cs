using PlexRichPresence.PlexActivity;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels.Test.Fakes;

public class FakePlexActivityService : IPlexActivityService
{
    public bool IsOwner { get; private set; }

    public bool IsConnected { get; private set; }

    public string? CurrentUserId { get; private set; }
    public string? CurrentServerIp { get; private set; }
    public int CurrentServerPort { get; private set; }
    public string? CurrentUserToken { get; private set; }
    
    public void Disconnect()
    {
        IsConnected = false;
    }

    public async IAsyncEnumerable<IPlexSession> GetSessions(bool isOwner, string userId, string serverIp, int serverPort, string userToken)
    {
        IsOwner = isOwner;
        CurrentUserId = userId;
        CurrentServerIp = serverIp;
        CurrentServerPort = serverPort;
        CurrentUserToken = userToken;
        IsConnected = true;


        for (int index = 1; index <= 3; ++index)
        {
            yield return new FakePlexSession { MediaTitle = $"Test Media Title {index}" };
        }
    }
}