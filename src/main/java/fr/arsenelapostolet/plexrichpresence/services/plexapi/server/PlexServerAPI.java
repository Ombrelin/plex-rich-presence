package fr.arsenelapostolet.plexrichpresence.services.plexapi.server;

import fr.arsenelapostolet.plexrichpresence.model.PlexSessions;
import retrofit2.http.GET;
import retrofit2.http.Header;
import rx.Observable;

public interface PlexServerAPI {

    @GET("/status/sessions")
    Observable<PlexSessions> getSessions(@Header("X-Plex-Token") String token, @Header("Accept") String accept);
}
