package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import lombok.Getter;
import lombok.Setter;
import retrofit2.Retrofit;

public abstract class PlexTvHttpClient {
    @Getter
    @Setter
    private Retrofit http;

    public PlexTvAPI getAPI() {
        return this.http.create(PlexTvAPI.class);
    }
}
