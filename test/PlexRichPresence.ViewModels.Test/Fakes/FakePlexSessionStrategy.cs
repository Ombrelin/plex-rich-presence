using PlexRichPresence.PlexActivity;

namespace PlexRichPresence.ViewModels.Test.Fakes;

public class FakePlexSessionStrategy : IPlexSessionStrategy
{
    public bool IsConnected { get; private set; }

    public string? CurrentUserId { get; private set; }
    public string? CurrentServerIp { get; private set; }
    public int CurrentServerPort { get; private set; }
    public string? CurrentUserToken { get; private set; }


    public async IAsyncEnumerable<PlexSession> GetSessions(string userId, string serverIp, int serverPort,
        string userToken)
    {
        CurrentUserId = userId;
        CurrentServerIp = serverIp;
        CurrentServerPort = serverPort;
        CurrentUserToken = userToken;
        IsConnected = true;


        for (int index = 0; index < 3; ++index)
        {
            yield return new PlexSession($"Test Media Title {++index}");
        }
    }

    public void Disconnect()
    {
        IsConnected = false;
    }
}