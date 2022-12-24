using DiscordRPC;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlexRichPresence.DiscordRichPresence.Rendering;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence;

public class DiscordService : IDiscordService
{
    private readonly ILogger<DiscordService> logger;
    private DiscordRpcClient? discordRpcClient;
    private readonly PlexSessionRenderingService plexSessionRenderingService;
    private IPlexSession? currentSession;

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

    public void SetDiscordPresenceToPlexSession(IPlexSession session)
    {
        if (JsonConvert.SerializeObject(session) == JsonConvert.SerializeObject(currentSession))
        {
            return;
        }
        currentSession = session;
        RichPresence richPresence = plexSessionRenderingService.RenderSession(session);
        richPresence.Assets = new Assets
        {
            LargeImageKey = "icon"
        };
        discordRpcClient ??= CreateRpcClient();
        discordRpcClient.SetPresence(richPresence);
    }

    public void StopRichPresence()
    {
        discordRpcClient?.Deinitialize();
        discordRpcClient = null;
        currentSession = null;
    }
}