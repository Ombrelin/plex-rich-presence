package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import fr.arsenelapostolet.plexrichpresence.model.PlexLogin;
import javafx.concurrent.Service;
import javafx.concurrent.Task;
import retrofit2.Call;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import retrofit2.converter.simplexml.SimpleXmlConverterFactory;

import java.io.IOException;

public class PlexTokenService extends Service<PlexLogin> {

    private static String API_URL = "https://plex.tv";

    private Retrofit http;
    private String login;
    private String password;

    public PlexTokenService(String login, String password) {
        this.login = login;
        this.password = password;

        Gson gson = new GsonBuilder()
                .setLenient()
                .create();

        this.http = new Retrofit.Builder()
                .baseUrl(API_URL)
                .addConverterFactory(GsonConverterFactory.create(gson))
                .build();


    }

    @Override
    protected Task<PlexLogin> createTask() {
        return new Task<PlexLogin>() {
            @Override
            protected PlexLogin call() {

                PlexAPI plexAPI = http.create(PlexAPI.class);

                Call<PlexLogin> ajax = plexAPI.login(login,password,
                        "Windows",
                        "10.0",
                        "Plex Rich Presence"
                );

                try {
                    Response<PlexLogin> result = ajax.execute();
                    if(result.errorBody() != null){
                        System.err.println(result.message());
                    }
                    return result.body();
                } catch (IOException e) {
                    e.printStackTrace();
                }
                return null;
            }
        };
    }
}
