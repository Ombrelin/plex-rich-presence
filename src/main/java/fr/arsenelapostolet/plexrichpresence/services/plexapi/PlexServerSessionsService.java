package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import fr.arsenelapostolet.plexrichpresence.model.PlexSessions;
import javafx.concurrent.Service;
import javafx.concurrent.Task;
import retrofit2.Call;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class PlexServerSessionsService extends Service<PlexSessions> {

    private Retrofit http;

    private String serverIp;
    private String serverPort;
    private String token;

    public PlexServerSessionsService(String serverIp, String serverPort, String token) {
        this.serverIp = serverIp;
        this.serverPort = serverPort;
        this.token = token;

        Gson gson = new GsonBuilder()
                .setLenient()
                .create();

        this.http = new Retrofit.Builder()
                .baseUrl("http://" + serverIp + ":" + serverPort)
                .addConverterFactory(GsonConverterFactory.create(gson))
                .build();
    }

    @Override
    protected Task<PlexSessions> createTask() {
        return new Task<PlexSessions>() {
            @Override
            protected PlexSessions call() throws Exception {
                PlexServerAPI plexServerAPI = http.create(PlexServerAPI.class);

                Call<PlexSessions> ajax = plexServerAPI.getSessions(token, "application/json");
                return ajax.execute().body();
            }
        };
    }
}
