using Websocket.Client;

namespace PlexRichPresence.PlexActivity;

public interface IWebSocketClientFactory
{
    WebsocketClient GetWebSocketClient(string serverIp, int serverPort, string userToken);
}