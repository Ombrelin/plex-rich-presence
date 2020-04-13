package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import fr.arsenelapostolet.plexrichpresence.model.MediaContainer;
import retrofit2.Call;
import retrofit2.http.GET;

public interface PlexAPI {

    @GET("https://plex.tv/pms/servers.xml")
    Call<MediaContainer> getServers();

}
