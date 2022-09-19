using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace PlexRichPresence.PlexActivity.Tests;

public class FakeWebSocketsServer
{
    public void Configure(IApplicationBuilder app, IHostingEnvironment _)
    {
        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        };

        app.UseWebSockets(webSocketOptions);

        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    for (var i = 0; i < 3; ++i)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        await webSocket.SendAsync(
                            Encoding.UTF8.GetBytes(
                                JsonSerializer.Serialize(
                                    new
                                    {
                                        NotificationContainer = new
                                        {
                                            type = "playing",
                                            PlaySessionStateNotification = new List<dynamic>
                                            {
                                                new { key = $"test-media-key-{i}" }
                                            }
                                        }
                                    }
                                )
                            ),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None
                        );
                    }
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            else
            {
                await next(context);
            }
        });
    }
}