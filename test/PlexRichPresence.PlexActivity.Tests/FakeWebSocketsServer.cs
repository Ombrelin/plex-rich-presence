using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace PlexRichPresence.PlexActivity.Tests;

public class FakeWebSocketsServer
{
    public static void Configure(IApplicationBuilder app)
    {
        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        };

        app.UseWebSockets(webSocketOptions);

        app.Use(async (context, next) =>
        {
            if (context.Request.Path != "/ws")
            {
                await next(context);
                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            for (var i = 0; i < 3; ++i)
            {
                await webSocket.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
                            new
                            {
                                NotificationContainer = new
                                {
                                    type = "playing",
                                    PlaySessionStateNotification = new List<dynamic>
                                    {
                                        new { key = $"test-media-key-{i}", state = "paused", viewOffset = 1000 }
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
        });
    }
}