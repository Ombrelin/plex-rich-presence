package fr.arsenelapostolet.plexrichpresence.services.plexapi.server;

import fr.arsenelapostolet.plexrichpresence.model.PlexSessions;
import javafx.concurrent.Service;
import javafx.concurrent.Task;
import lombok.AllArgsConstructor;
import retrofit2.Call;

@AllArgsConstructor
public class PlexSessionAjax extends Service<PlexSessions> {

    private PlexServerAPI plexServerAPI;
    private String token;

    @Override
    protected Task<PlexSessions> createTask() {
        return new Task<PlexSessions>() {
            @Override
            protected PlexSessions call() throws Exception {
                Call<PlexSessions> ajax = plexServerAPI.getSessions(token, "application/json");
                return ajax.execute().body();
            }
        };
    }

}
