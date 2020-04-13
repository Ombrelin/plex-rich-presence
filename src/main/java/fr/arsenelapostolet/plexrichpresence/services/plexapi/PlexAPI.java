package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import fr.arsenelapostolet.plexrichpresence.model.MediaContainer;
import fr.arsenelapostolet.plexrichpresence.model.PlexLogin;
import retrofit2.Call;
import retrofit2.http.GET;
import retrofit2.http.Header;
import retrofit2.http.POST;
import retrofit2.http.Query;

public interface PlexAPI {

    @POST("/users/sign_in.json")
    Call<PlexLogin> login(@Query("user[login]") String login,
                          @Query("user[password]") String password,
                          @Header("X-Plex-Product") String os,
                          @Header("X-Plex-Version") String version,
                          @Header("X-Plex-Client-Identifier") String client);

    @GET("/pms/servers.xml")
    Call<MediaContainer> getServers();

}
