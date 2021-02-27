package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import retrofit2.Retrofit;

public abstract class PlexTvHttpClient {

    private Retrofit http;
    public OkHttpClient.Builder httpClientFactory = new OkHttpClient.Builder();
    public Logger LOG = LoggerFactory.getLogger(PlexTvHttpClient.class);
    public HttpLoggingInterceptor logging = new HttpLoggingInterceptor(LOG::debug).setLevel(HttpLoggingInterceptor.Level.BODY);

    public PlexTvHttpClient() {
        httpClientFactory.addInterceptor(logging);
    }

    public PlexTvAPI getAPI() {
        return this.http.create(PlexTvAPI.class);
    }

    protected void setHttp(Retrofit http) {
        this.http = http;
    }
}
