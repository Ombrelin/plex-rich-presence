package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import fr.arsenelapostolet.plexrichpresence.model.MediaContainer;
import javafx.concurrent.Service;
import javafx.concurrent.Task;
import okhttp3.OkHttpClient;
import retrofit2.Call;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.simplexml.SimpleXmlConverterFactory;

import java.io.IOException;

public class PlexServerService extends Service<MediaContainer> {

    private static String API_URL = "https://plex.tv";

    private OkHttpClient.Builder httpClient = new OkHttpClient.Builder();

    private Retrofit http;
    private String login;
    private String password;

    public PlexServerService(String login, String password) {
        this.login = login;
        this.password = password;

        BasicAuthInterceptor interceptor =
                new BasicAuthInterceptor(login, password);
        httpClient.addInterceptor(interceptor);

        this.http = new Retrofit.Builder()
                .client(this.httpClient.build())
                .baseUrl(API_URL)
                .addConverterFactory(SimpleXmlConverterFactory.create())
                .build();


    }

    @Override
    protected Task<MediaContainer> createTask() {
        return new Task<MediaContainer>() {
            @Override
            protected MediaContainer call() throws IOException {
                PlexAPI plexAPI = http.create(PlexAPI.class);
                Call<MediaContainer> ajax = plexAPI.getServers();
                Response<MediaContainer> response = ajax.execute();


                if (response.errorBody() != null) {
                    throw new IllegalArgumentException(response.errorBody().string());
                }
                if (response.code() == 401) {
                    throw new IllegalArgumentException("Wrong credentials");
                }
                return response.body();
            }
        };
    }
}
