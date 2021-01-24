package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import retrofit2.Retrofit;
import retrofit2.adapter.rxjava.RxJavaCallAdapterFactory;
import retrofit2.converter.gson.GsonConverterFactory;

public class PlexTokenHttpClient extends PlexTvHttpClient {

    private static String API_URL = "https://plex.tv";


    public PlexTokenHttpClient() {

        Gson gson = new GsonBuilder()
                .setLenient()
                .create();

        this.setHttp(new Retrofit.Builder()
                .baseUrl(API_URL)
                .addConverterFactory(GsonConverterFactory.create(gson))
                .addCallAdapterFactory(RxJavaCallAdapterFactory.create())
                .build());

    }


}
