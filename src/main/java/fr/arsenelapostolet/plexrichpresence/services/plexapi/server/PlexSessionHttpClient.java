package fr.arsenelapostolet.plexrichpresence.services.plexapi.server;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import retrofit2.Retrofit;
import retrofit2.adapter.rxjava.RxJavaCallAdapterFactory;
import retrofit2.converter.gson.GsonConverterFactory;

public class PlexSessionHttpClient {

    private Retrofit http;

    public PlexSessionHttpClient(String serverIp, String serverPort) {

        Gson gson = new GsonBuilder()
                .setLenient()
                .create();

        this.http = new Retrofit.Builder()
                .baseUrl("http://" + serverIp + ":" + serverPort)
                .addConverterFactory(GsonConverterFactory.create(gson))
                .addCallAdapterFactory(RxJavaCallAdapterFactory.create())
                .build();
    }

    public PlexServerAPI getAPI(){
        return this.http.create(PlexServerAPI.class);
    }
}
