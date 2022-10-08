using Websocket.Client;

namespace PlexRichPresence.PlexActivity;

public class WebSocketClientFactory : IWebSocketClientFactory
{
    public WebsocketClient GetWebSocketClient(string serverIp, int serverPort, string userToken)
    {
        var uri = new Uri($"ws://{serverIp}:{serverPort}/:/websockets/notifications?X-Plex-Token={userToken}");
        return new WebsocketClient(uri);
    }
}