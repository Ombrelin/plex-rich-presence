namespace PlexRichPresence.DiscordGameSDK;

public class DiscordGameSdk
{
    private readonly Discord.Discord discord;

    public DiscordGameSdk()
    {
        discord = new Discord.Discord(698954724019273770, (ulong) Discord.CreateFlags.Default);
    }

    public void UpdateActivity()
    {
        
        var activityManager = discord.GetActivityManager();
        var lobbyManager = discord.GetLobbyManager();

        var activity = new Discord.Activity
        {
            State = "olleh",
            Details = "foo details",
            Timestamps =
            {
                Start = 5,
                End = 6,
            },
            // Assets =
            // {
            //     LargeImage = "foo largeImageKey",
            //     LargeText = "foo largeImageText",
            //     SmallImage = "foo smallImageKey",
            //     SmallText = "foo smallImageText",
            // },
            Instance = true,
        };
    }
}