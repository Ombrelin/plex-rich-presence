package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import fr.arsenelapostolet.plexrichpresence.model.MediaContainerServer;
import fr.arsenelapostolet.plexrichpresence.model.PlexLogin;
import retrofit2.Call;
import retrofit2.http.GET;
import retrofit2.http.Header;
import retrofit2.http.POST;
import retrofit2.http.Query;

public interface PlexTvAPI {

    @POST("/users/sign_in.json")
    Call<PlexLogin> login(@Query("user[login]") String login,
                          @Query("user[password]") String password,
                          @Header("X-Plex-Product") String os,
                          @Header("X-Plex-Version") String version,
                          @Header("X-Plex-Client-Identifier") String client);

    @GET("/pms/servers.xml")
    Call<MediaContainerServer> getServers();

}
