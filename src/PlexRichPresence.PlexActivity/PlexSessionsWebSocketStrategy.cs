using System.Net.WebSockets;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Plex.ServerApi.PlexModels.Media;
using PlexRichPresence.ViewModels.Services;
using Websocket.Client;

namespace PlexRichPresence.PlexActivity;

public class PlexSessionsWebSocketStrategy : IPlexSessionStrategy
{
    private readonly ILogger logger;
    private IWebsocketClient? client;
    private readonly IPlexServerClient plexServerClient;

    public PlexSessionsWebSocketStrategy(ILogger logger)
    {
        this.logger = logger;
    }

    public IObservable<PlexSession> GetSessions(string serverIp, int serverPort, string userToken)
    {
        var uri = new Uri($"ws://{serverIp}:{serverPort}/:/websockets/notifications?X-Plex-Token={userToken}");
        client?.Dispose();

        client = new WebsocketClient(uri);

        client.DisconnectionHappened.Subscribe(HandleDisconnection);
        
        IObservable<JsonNode> notifications = client.MessageReceived
            .Select(ExtractNotification)
            .Where(IsPlayingNotification);

        notifications
            .Subscribe(notification => this.logger.LogInformation(notification.ToJsonString()));
        
        notifications
            .SelectMany(ExtractSession)
            .Select(ExtractMediaKey)
            .Select(mediaKey => this.GetMediaFromKey(mediaKey, userToken, serverIp, serverPort))
            .Subscribe(RaiseEventWithMedia, HandleError);
        client.Start();
    }

    private async void RaiseEventWithMedia(Task<MediaContainer> mediaContainerTask)
    {
        MediaContainer mediaContainer = (await mediaContainerTask);
        Metadata media = mediaContainer.Media.First();

    }

    public Task<MediaContainer> GetMediaFromKey(string mediaKey, string userToken, string serverIp, int serverPort)
    {
        return this.plexServerClient.GetMediaMetadataAsync(
            userToken,
            new Uri($"http://{serverIp}:{serverPort}").ToString(),
            mediaKey
        );
    }
    
    private void HandleDisconnection(DisconnectionInfo disconnectionInfo)
    {
        this.OnDisconnection?.Invoke(this, null);
        this.logger.LogWarning(JsonConvert.SerializeObject(disconnectionInfo));
    }
    
    private string ExtractMediaKey(JsonNode message)
    {
        JsonNode key = message["key"] ?? throw new ArgumentException("Media has no key");
        return key.GetValue<string>().Split("/").Last();
    }

    private IEnumerable<JsonNode> ExtractSession(JsonNode message)
    {
        JsonNode sessions = message["PlaySessionStateNotification"] ?? throw new ArgumentException("Notification has no sessions");
        return sessions.AsArray();
    }
    
    private JsonNode ExtractNotification(ResponseMessage message)
    {
        JsonNode webSocketMessage = JsonNode.Parse(message.Text) ?? throw new ArgumentException("Can't parse WebSocket message as JSON");
        return webSocketMessage["NotificationContainer"] ?? throw new ArgumentException("WebSocket message has no notification");
    }

    private bool IsPlayingNotification(JsonNode message)
    {
        JsonNode type = message["type"] ?? throw new ArgumentException("Notification has no type");
        return type.GetValue<string>() is "playing";
    }
    
    public void Disconnect()
    {
        client?.Stop(WebSocketCloseStatus.NormalClosure, "Stopped");
        client?.Dispose();
    }
}