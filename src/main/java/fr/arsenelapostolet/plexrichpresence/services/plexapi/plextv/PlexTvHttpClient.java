package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import retrofit2.Retrofit;

public abstract class PlexTvHttpClient {

    private Retrofit http;

    public PlexTvAPI getAPI() {
        return this.http.create(PlexTvAPI.class);
    }

    protected void setHttp(Retrofit http) {
        this.http = http;
    }
}
