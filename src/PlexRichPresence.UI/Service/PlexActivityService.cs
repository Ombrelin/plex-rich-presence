using System.Net.WebSockets;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.UI.Service;

public class PlexActivityService : IPlexActivityService
{
    private ClientWebSocket client = new ClientWebSocket();
    
    public void Connect(string serverIp, string userToken)
    {
        throw new NotImplementedException();
    }

    public event EventHandler OnActivityUpdated;
}