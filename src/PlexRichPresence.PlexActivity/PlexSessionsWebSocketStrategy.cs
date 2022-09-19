using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Media;
using Websocket.Client;

namespace PlexRichPresence.PlexActivity;

public class PlexSessionsWebSocketStrategy : IPlexSessionStrategy
{
    private readonly ILogger logger;
    private IWebsocketClient? client;
    private readonly IPlexServerClient plexServerClient;
    private IWebSocketClientFactory webSocketClientFactory;

    public PlexSessionsWebSocketStrategy(
        ILogger logger,
        IPlexServerClient plexServerClient,
        IWebSocketClientFactory webSocketClientFactory
    )
    {
        this.logger = logger;
        this.plexServerClient = plexServerClient;
        this.webSocketClientFactory = webSocketClientFactory;
    }

    public async IAsyncEnumerable<PlexSession> GetSessions(string _, string serverIp, int serverPort, string userToken)
    {
        client?.Dispose();
        client = webSocketClientFactory.GetWebSocketClient(serverIp, serverPort, userToken);

        client.DisconnectionHappened.Subscribe(HandleDisconnection);

        var mediaKeys = client.MessageReceived
            .Select(ExtractNotification)
            .Where(IsPlayingNotification)
            .SelectMany(ExtractSession)
            .Select(ExtractMediaKey)
            .ToAsyncEnumerable();

        await client.Start();

        await foreach (string mediaKey in mediaKeys)
        {
            yield return await ExtractPlexSession(serverIp, serverPort, userToken, mediaKey);
        }
    }

    private async Task<PlexSession> ExtractPlexSession(string serverIp, int serverPort, string userToken,
        string mediaKey)
    {
        MediaContainer mediaContainer = await this.GetMediaFromKey(mediaKey, userToken, serverIp, serverPort);
        Metadata media = mediaContainer.Media.First();
        return new PlexSession(media.Title);
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
        throw disconnectionInfo.Exception;
    }

    private string ExtractMediaKey(JsonNode message)
    {
        JsonNode key = message["key"] ?? throw new ArgumentException("Media has no key");
        return key.GetValue<string>().Split("/").Last();
    }

    private IEnumerable<JsonNode> ExtractSession(JsonNode message)
    {
        JsonNode sessions = message["PlaySessionStateNotification"] ??
                            throw new ArgumentException("Notification has no sessions");
        return sessions.AsArray();
    }

    private JsonNode ExtractNotification(ResponseMessage message)
    {
        JsonNode webSocketMessage = JsonNode.Parse(message.Text) ??
                                    throw new ArgumentException("Can't parse WebSocket message as JSON");
        return webSocketMessage["NotificationContainer"] ??
               throw new ArgumentException("WebSocket message has no notification");
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


