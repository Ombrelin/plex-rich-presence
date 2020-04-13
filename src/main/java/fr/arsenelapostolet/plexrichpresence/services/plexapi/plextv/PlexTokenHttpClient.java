package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class PlexTokenHttpClient extends PlexTvHttpClient {

    private static String API_URL = "https://plex.tv";

    private String login;
    private String password;

    public PlexTokenHttpClient(String login, String password) {
        this.login = login;
        this.password = password;

        Gson gson = new GsonBuilder()
                .setLenient()
                .create();

        this.setHttp(new Retrofit.Builder()
                .baseUrl(API_URL)
                .addConverterFactory(GsonConverterFactory.create(gson))
                .build());


    }


}
