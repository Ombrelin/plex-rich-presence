using DiscordRPC;
using Microsoft.Extensions.Logging;
using PlexRichPresence.Core;
using PlexRichPresence.DiscordRichPresence.Rendering;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence;

public class DiscordService : IDiscordService
{
    private readonly ILogger<DiscordService> logger;
    private DiscordRpcClient? discordRpcClient;
    private readonly PlexSessionRenderingService plexSessionRenderingService;
    private PlexSession? currentSession;
    private CancellationTokenSource stopTokenSource = new();
    private bool stopFlag = false;

    public DiscordService(ILogger<DiscordService> logger, PlexSessionRenderingService plexSessionRenderingService)
    {
        this.logger = logger;
        this.plexSessionRenderingService = plexSessionRenderingService;
        this.discordRpcClient = CreateRpcClient();
    }

    private DiscordRpcClient CreateRpcClient()
    {
        var rpcClient = new DiscordRpcClient(applicationID: "698954724019273770");
        rpcClient.OnError += (sender, args) => this.logger.LogError(args.Message);
        rpcClient.Initialize();

        return rpcClient;
    }

    public void SetDiscordPresenceToPlexSession(PlexSession session)
    {
        if (session == currentSession)
        {
            return;
        }

        if (stopFlag) stopTokenSource.Cancel();

        currentSession = session;
        RichPresence richPresence = plexSessionRenderingService.RenderSession(session);
        discordRpcClient ??= CreateRpcClient();
        discordRpcClient.SetPresence(richPresence);
    }

    public async void StopRichPresence()
    {
        if (stopFlag) return; // If we are already stopping, dont try again (race conditions, yay!)
        stopFlag = true;

        // Tbh this is kinda ugly... But I dont think there is a better way since the actual session in plex closes for like 500ms between tracks
        try
        {
            await Task.Delay(10_000, stopTokenSource.Token);

            discordRpcClient?.Deinitialize();
            discordRpcClient = null;
            currentSession = null;
        }
        catch (TaskCanceledException) { } // Throws and crashes if this is not here
        finally
        {
            stopTokenSource.Dispose();
            stopTokenSource = new();
            stopFlag = false;
        }
    }
}