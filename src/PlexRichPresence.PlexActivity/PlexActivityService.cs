using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Media;
using PlexRichPresence.ViewModels.Services;
using Websocket.Client;

namespace PlexRichPresence.PlexActivity;

public class PlexActivityService : IPlexActivityService
{

    private readonly ILogger logger;

    public PlexActivityService(IPlexServerClient plexServerClient, ILogger logger)
    {
        this.plexServerClient = plexServerClient;
        this.logger = logger;
    }

    public void Connect(string serverIp, int serverPort, string userToken, bool isOwner)
    {
        

            .Subscribe(RaiseEventWithMedia, HandleError);
            this.OnActivityUpdated?.Invoke(
                this,
                new IPlexActivityService.PlexActivityEventArg { CurrentActivity = $"{media.Title} - {media.ParentTitle}" }
            );
    }

    private void HandleError(Exception e)
    {
        this.OnDisconnection?.Invoke(this, null);
        this.logger.LogWarning(JsonConvert.SerializeObject(e));
    }
    




    public event EventHandler? OnActivityUpdated;
    public event EventHandler? OnDisconnection;

    public void Disconnect()
    {
        client?.Stop(WebSocketCloseStatus.NormalClosure, "Stopped");
        client?.Dispose();
    }


}