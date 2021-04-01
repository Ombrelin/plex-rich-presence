package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import fr.arsenelapostolet.plexrichpresence.model.MediaContainerServer;
import fr.arsenelapostolet.plexrichpresence.model.PlexAuth;
import fr.arsenelapostolet.plexrichpresence.model.PlexLogin;
import fr.arsenelapostolet.plexrichpresence.model.User;
import retrofit2.http.*;
import rx.Observable;

public interface PlexTvAPI {

    @POST("/users/sign_in.json")
    Observable<PlexLogin> login(@Query("user[login]") String login,
                                @Query("user[password]") String password,
                                @Header("X-Plex-Product") String os,
                                @Header("X-Plex-Version") String version,
                                @Header("X-Plex-Client-Identifier") String client);

    @GET("/pms/servers.xml")
    Observable<MediaContainerServer> getServers(@Query("X-Plex-Token") String plexToken);

    @POST("/api/v2/pins.json")
    Observable<PlexAuth> generatePlexAuthPin(@Query("strong") boolean strong,
                                             @Query("X-Plex-Product") String plexProduct,
                                             @Query("X-Plex-Client-Identifier") String plexIdentifier);

    @GET("/api/v2/pins/{id}.json")
    Observable<PlexAuth> validatePlexAuthPin(@Path("id") int id,
                                             @Query("X-Plex-Client-Identifier") String plexIdentifier,
                                             @Query("code") String code);

    @GET("/api/v2/user.json")
    Observable<User> getUser(@Query("X-Plex-Token") String plexToken,
                             @Query("X-Plex-Client-Identifier") String plexIdentifier,
                             @Query("X-Plex-Product") String plexProduct);


}
