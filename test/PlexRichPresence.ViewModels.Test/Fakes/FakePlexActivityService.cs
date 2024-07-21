using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels.Test.Fakes;

public class FakePlexActivityService : IPlexActivityService
{
    public bool IsOwner { get; private set; }

    public bool IsConnected { get; private set; }

    public string? CurrentUsername { get; private set; }
    public string? CurrentServerIp { get; private set; }
    public int CurrentServerPort { get; private set; }
    public string? CurrentUserToken { get; private set; }

    private readonly bool _isIdle;

    public FakePlexActivityService(bool isIdle = false) => _isIdle = isIdle;

    public void Disconnect() => IsConnected = false;

    public async IAsyncEnumerable<PlexSession> GetSessions(bool isOwner, string userId, string serverIp, int serverPort, string userToken)
    {
        IsOwner = isOwner;
        CurrentUsername = userId;
        CurrentServerIp = serverIp;
        CurrentServerPort = serverPort;
        CurrentUserToken = userToken;
        IsConnected = true;


        for (var index = 1; index <= 3; ++index)
            yield return _isIdle ? new PlexSession() : new PlexSession { MediaTitle = $"Test Media Title {index}" };
    }
}