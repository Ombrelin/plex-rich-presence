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
    private readonly ILogger<PlexSessionsWebSocketStrategy> _logger;
    private IWebsocketClient? _client;
    private readonly IPlexServerClient _plexServerClient;
    private readonly IWebSocketClientFactory _webSocketClientFactory;
    private readonly PlexSessionMapper _plexSessionMapper;

    public PlexSessionsWebSocketStrategy(ILogger<PlexSessionsWebSocketStrategy> logger, IPlexServerClient plexServerClient, IWebSocketClientFactory webSocketClientFactory, PlexSessionMapper plexSessionMapper)
    {
        _logger = logger;
        _plexServerClient = plexServerClient;
        _webSocketClientFactory = webSocketClientFactory;
        _plexSessionMapper = plexSessionMapper;
    }

    public async IAsyncEnumerable<PlexSession> GetSessions(string username, string serverIp, int serverPort, string userToken)
    {
        _client?.Dispose();
        _client = _webSocketClientFactory.GetWebSocketClient(serverIp, serverPort, userToken);

        var sessions = _client.MessageReceived
            .Select(ExtractNotification)
            .Where(IsPlayingNotification)
            .SelectMany(ExtractSession)
            .Select(ExtractSessionData)
            .ToAsyncEnumerable();

        await _client.Start();

        _logger.LogInformation("Listening to sessions via websocket for user : {Username}", username);
        await foreach (var (key, state, viewOffset) in sessions)
            yield return await ExtractPlexSession(serverIp, serverPort, userToken, key, state, viewOffset);
    }

    private async Task<PlexSession> ExtractPlexSession(string serverIp, int serverPort, string userToken, string mediaKey, string state, long viewOffset)
    {
        var mediaContainer = await GetMediaFromKey(mediaKey, userToken, serverIp, serverPort);
        var media = mediaContainer.Media.First();
        var plexServerHost = new Uri($"http://{serverIp}:{serverPort}").ToString();
        
        return _plexSessionMapper.Map(media, state, viewOffset, plexServerHost, userToken);
    }

    private Task<MediaContainer> GetMediaFromKey(string mediaKey, string userToken, string serverIp, int serverPort)
    {
        return _plexServerClient.GetMediaMetadataAsync(userToken, new Uri($"http://{serverIp}:{serverPort}").ToString(), mediaKey);
    }

    private (string key, string state, long viewOffset) ExtractSessionData(JsonNode message)
    {
        _logger.LogInformation("Websocket session : {Session}", message.ToJsonString());
        var key = message["key"] ?? throw new ArgumentException("Media has no key");
        var state = message["state"] ?? throw new ArgumentException("Media has no state");
        var viewOffset = message["viewOffset"] ?? throw new ArgumentException("Media has no viewOffset");
        return (key.GetValue<string>().Split("/").Last(), state.GetValue<string>(), viewOffset.GetValue<long>());
    }

    private IEnumerable<JsonNode> ExtractSession(JsonNode message)
    {
        var sessions = message["PlaySessionStateNotification"] ?? throw new ArgumentException("Notification has no sessions");
        return sessions.AsArray()!;
    }

    private JsonNode ExtractNotification(ResponseMessage message)
    {
        _logger.LogInformation("Detected session");
        var webSocketMessage = JsonNode.Parse(message.Text) ?? throw new ArgumentException("Can't parse WebSocket message as JSON");
        return webSocketMessage["NotificationContainer"] ?? throw new ArgumentException("WebSocket message has no notification");
    }

    private bool IsPlayingNotification(JsonNode message)
    {
        var type = message["type"] ?? throw new ArgumentException("Notification has no type");
        return type.GetValue<string>() is "playing";
    }

    public void Disconnect()
    {
        _client?.Stop(WebSocketCloseStatus.NormalClosure, "Stopped");
        _client?.Dispose();
    }
}