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

import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.SSLSession;

public class PlexSessionHttpClient {

    private final Retrofit http;
    private final Logger LOG = LoggerFactory.getLogger(PlexSessionHttpClient.class);

    public PlexSessionHttpClient(String serverIp, String serverPort, boolean secure) {

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
        httpClient.hostnameVerifier(new HostnameVerifier() {
            // Potentially unsafe. Plex SSL certificates are self signed/not trusted. Will cause javax.net.ssl.SSLPeerUnverifiedException without it.
            @Override
            public boolean verify(String hostname, SSLSession session) {
                return true;
            }
        });
        httpClient.addInterceptor(logging);

        String baseURL = "http://" + serverIp + ":" + serverPort;
        if (secure) {
            baseURL = "https://" + serverIp + ":" + serverPort;
        }

        this.http = new Retrofit.Builder()
                .baseUrl(baseURL)
                .addConverterFactory(GsonConverterFactory.create(gson))
                .addCallAdapterFactory(RxJavaCallAdapterFactory.create())
                .client(httpClient.build())
                .build();
    }

    public PlexServerAPI getAPI() {
        return this.http.create(PlexServerAPI.class);
    }
}
