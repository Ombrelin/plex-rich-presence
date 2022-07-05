namespace PlexRichPresence.ViewModels.Services;

public interface IPlexActivityService
{
    void Connect(string serverIp, string userToken);
    event EventHandler OnActivityUpdated;

    class PlexActivityEventArg : EventArgs
    {
        public string CurrentActivity { get; set; }
    }
}