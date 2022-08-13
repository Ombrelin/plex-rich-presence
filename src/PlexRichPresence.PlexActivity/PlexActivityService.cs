using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Media;
using PlexRichPresence.ViewModels.Services;
using Websocket.Client;

namespace PlexRichPresence.PlexActivity;

public class PlexActivityService : IPlexActivityService
{
    private IWebsocketClient client;
    private readonly IPlexServerClient plexServerClient;

    public PlexActivityService(IPlexServerClient plexServerClient)
    {
        this.plexServerClient = plexServerClient;
    }

    public void Connect(string serverIp, int serverPort, string userToken)
    {
        var uri = new Uri($"ws://{serverIp}:{serverPort}/:/websockets/notifications?X-Plex-Token={userToken}");
        if (client is not null)
        {
            client.Dispose();
        }

        client = new WebsocketClient(uri);
        IObservable<JsonNode?> observable = client.MessageReceived
            .Select(message => JsonNode.Parse(message.Text)?["NotificationContainer"])
            .Where(message => message["type"].GetValue<string>() is "playing");

        client.MessageReceived
            .Where(message => JsonNode.Parse(message.Text)?["NotificationContainer"]["type"].GetValue<string>() is "playing")
            .Subscribe(notif => Console.WriteLine(notif.Text));
        
        observable
            .SelectMany(message => message["PlaySessionStateNotification"].AsArray())
            .Select(message => message["key"].GetValue<string>().Split("/").Last())
            .Select(mediaKey => this.plexServerClient.GetMediaMetadataAsync(
                userToken,
                new Uri($"http://{serverIp}:{serverPort}").ToString(),
                mediaKey
                )
            )
            .Subscribe(async mediaContainerTask
                    =>
                {
                    MediaContainer mediaContainer = (await mediaContainerTask);
                    //Console.WriteLine(JsonConvert.SerializeObject(mediaContainer));
                    var media = mediaContainer.Media.First();
                    this.OnActivityUpdated?.Invoke(
                        this,
                        new IPlexActivityService.PlexActivityEventArg { CurrentActivity = $"{media.Title} - {media.ParentTitle}" }
                    );
                }
                );
        client.Start();
    }

    public event EventHandler? OnActivityUpdated;

    public void Disconnect()
    {
        client.Stop(WebSocketCloseStatus.NormalClosure, "Stopped");
        client.Dispose();
    }
}