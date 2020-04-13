package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import fr.arsenelapostolet.plexrichpresence.model.PlexLogin;
import javafx.concurrent.Service;
import javafx.concurrent.Task;
import lombok.AllArgsConstructor;
import retrofit2.Call;
import retrofit2.Response;

import java.io.IOException;

@AllArgsConstructor
public class PlexTokenAjax extends Service<PlexLogin> {

    private PlexTvAPI plexTvAPI;
    private String login;
    private String password;

    @Override
    protected Task<PlexLogin> createTask() {
        return new Task<PlexLogin>() {
            @Override
            protected PlexLogin call() {

                Call<PlexLogin> ajax = plexTvAPI.login(login,password,
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
