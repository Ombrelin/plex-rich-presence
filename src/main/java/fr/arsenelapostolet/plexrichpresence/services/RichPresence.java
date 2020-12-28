package fr.arsenelapostolet.plexrichpresence.services;

import club.minnced.discord.rpc.DiscordEventHandlers;
import club.minnced.discord.rpc.DiscordRPC;
import club.minnced.discord.rpc.DiscordRichPresence;
import org.springframework.stereotype.Service;

@Service
public class RichPresence {

    private static final String token = "698954724019273770";
    private DiscordRichPresence presence = new DiscordRichPresence();
    private DiscordRPC lib = DiscordRPC.INSTANCE;

    public RichPresence() {
        String steamId = "";
        DiscordEventHandlers handlers = new DiscordEventHandlers();
        handlers.ready = (user) -> System.out.println("Ready!");
        lib.Discord_Initialize(token, handlers, true, steamId);

        presence.startTimestamp = System.currentTimeMillis() / 1000; // epoch second
        presence.details = "Idling";
        presence.largeImageKey = "icon";
        lib.Discord_UpdatePresence(presence);
    }

    public void updateMessage(String state, String media){
        presence.details = state;
        presence.state = media;
        lib.Discord_UpdatePresence(presence);
    }

    public void setEndTimestamp(long end) {
         presence.endTimestamp = end;
        lib.Discord_UpdatePresence(presence);
    }

    public void setStartTimestamp(long start) {
        presence.startTimestamp = start;
        lib.Discord_UpdatePresence(presence);
    }

}
