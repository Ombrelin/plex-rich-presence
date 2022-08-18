namespace PlexRichPresence.ViewModels.Services;

public interface IPlexActivityService
{
    void Connect(string serverIp, int serverPort, string userToken, bool isOwner);

    void Disconnect();
    event EventHandler OnActivityUpdated;
    event EventHandler OnDisconnection;

    class PlexActivityEventArg : EventArgs
    {
        public string CurrentActivity { get; set; }
    }
}