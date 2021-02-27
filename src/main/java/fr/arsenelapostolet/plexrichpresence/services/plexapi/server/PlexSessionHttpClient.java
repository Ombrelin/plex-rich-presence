package fr.arsenelapostolet.plexrichpresence.services.plexapi.server;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import retrofit2.Retrofit;
import retrofit2.adapter.rxjava.RxJavaCallAdapterFactory;
import retrofit2.converter.gson.GsonConverterFactory;

public class PlexSessionHttpClient {

    private Retrofit http;
    private final Logger LOG = LoggerFactory.getLogger(PlexSessionHttpClient.class);

    public PlexSessionHttpClient(String serverIp, String serverPort) {

        final Gson gson = new GsonBuilder()
                .setLenient()
                .create();

        HttpLoggingInterceptor logging = new HttpLoggingInterceptor(new HttpLoggingInterceptor.Logger() {
            @Override
            public void log(String message) {
                LOG.debug(message);
            }
        });

        logging.setLevel(HttpLoggingInterceptor.Level.BODY);

        OkHttpClient.Builder httpClient = new OkHttpClient.Builder();
        httpClient.addInterceptor(logging);

        this.http = new Retrofit.Builder()
                .baseUrl("http://" + serverIp + ":" + serverPort)
                .addConverterFactory(GsonConverterFactory.create(gson))
                .addCallAdapterFactory(RxJavaCallAdapterFactory.create())
                .client(httpClient.build())
                .build();
    }

    public PlexServerAPI getAPI() {
        return this.http.create(PlexServerAPI.class);
    }
}
