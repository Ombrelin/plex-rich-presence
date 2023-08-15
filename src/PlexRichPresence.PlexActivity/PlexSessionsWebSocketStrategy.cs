using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Media;
using PlexRichPresence.Core;
using Websocket.Client;

namespace PlexRichPresence.PlexActivity;

public class PlexSessionsWebSocketStrategy : IPlexSessionStrategy
{
    private readonly ILogger<PlexSessionsWebSocketStrategy> logger;
    private IWebsocketClient? client;
    private readonly IPlexServerClient plexServerClient;
    private readonly IWebSocketClientFactory webSocketClientFactory;
    private readonly PlexSessionMapper plexSessionMapper;

    public PlexSessionsWebSocketStrategy(
        ILogger<PlexSessionsWebSocketStrategy> logger,
        IPlexServerClient plexServerClient,
        IWebSocketClientFactory webSocketClientFactory,
        PlexSessionMapper plexSessionMapper)
    {
        this.logger = logger;
        this.plexServerClient = plexServerClient;
        this.webSocketClientFactory = webSocketClientFactory;
        this.plexSessionMapper = plexSessionMapper;
    }

    public async IAsyncEnumerable<PlexSession> GetSessions(string username, string serverIp, int serverPort,
        string userToken)
    {
        client?.Dispose();
        client = webSocketClientFactory.GetWebSocketClient(serverIp, serverPort, userToken);

        IAsyncEnumerable<(string key, string state, long viewOffset)> sessions = client.MessageReceived
            .Select(ExtractNotification)
            .Where(IsPlayingNotification)
            .SelectMany(ExtractSession)
            .Select(ExtractSessionData)
            .ToAsyncEnumerable();

        await client.Start();

        logger.LogInformation("Listening to sessions via websocket for user : {Username}", username);
        await foreach ((string key, string state, long viewOffset) in sessions)
        {
            yield return await ExtractPlexSession(serverIp, serverPort, userToken, key, state, viewOffset);
        }
    }

    private async Task<PlexSession> ExtractPlexSession(string serverIp, int serverPort, string userToken,
        string mediaKey, string state, long viewOffset)
    {
        MediaContainer mediaContainer = await this.GetMediaFromKey(mediaKey, userToken, serverIp, serverPort);
        Metadata media = mediaContainer.Media.First();
        var plexServerHost = new Uri($"http://{serverIp}:{serverPort}").ToString();
        return plexSessionMapper.Map(
            media,
            state,
            viewOffset, plexServerHost, userToken);
    }

    private Task<MediaContainer> GetMediaFromKey(string mediaKey, string userToken, string serverIp, int serverPort)
    {
        return this.plexServerClient.GetMediaMetadataAsync(
            userToken,
            new Uri($"http://{serverIp}:{serverPort}").ToString(),
            mediaKey
        );
    }

    private (string key, string state, long viewOffset) ExtractSessionData(JsonNode message)
    {
        this.logger.LogInformation("Websocket session : {Session}", message.ToJsonString());
        JsonNode key = message["key"] ?? throw new ArgumentException("Media has no key");
        JsonNode state = message["state"] ?? throw new ArgumentException("Media has no state");
        JsonNode viewOffset = message["viewOffset"] ?? throw new ArgumentException("Media has no viewOffset");
        return (key.GetValue<string>().Split("/").Last(), state.GetValue<string>(), viewOffset.GetValue<long>());
    }

    private IEnumerable<JsonNode> ExtractSession(JsonNode message)
    {
        JsonNode sessions = message["PlaySessionStateNotification"] ??
                            throw new ArgumentException("Notification has no sessions");
        return sessions.AsArray()!;
    }

    private JsonNode ExtractNotification(ResponseMessage message)
    {
        this.logger.LogInformation("Detected session");
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