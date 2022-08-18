namespace PlexRichPresence.PlexActivity;

public interface IPlexSessionStrategy
{
    IObservable<PlexSession> GetSessions(string serverIp, int serverPort, string userToken);
    void Disconnect();
}