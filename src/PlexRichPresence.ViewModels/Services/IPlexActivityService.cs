namespace PlexRichPresence.ViewModels.Services;

public interface IPlexActivityService
{
    void Connect(string serverIp, int serverPort, string userToken);

    void Disconnect();
    event EventHandler OnActivityUpdated;

    class PlexActivityEventArg : EventArgs
    {
        public string CurrentActivity { get; set; }
    }
}