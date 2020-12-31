package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import fr.arsenelapostolet.plexrichpresence.model.MediaContainerServer;
import javafx.concurrent.Service;
import javafx.concurrent.Task;
import lombok.AllArgsConstructor;
import retrofit2.Call;
import retrofit2.Response;

import java.io.IOException;

@AllArgsConstructor
public class PlexServerAjax extends Service<MediaContainerServer> {

    private PlexTvAPI plexTvAPI;

    @Override
    protected Task<MediaContainerServer> createTask() {
        return new Task<MediaContainerServer>() {
            @Override
            protected MediaContainerServer call() throws IOException {
                Call<MediaContainerServer> ajax = plexTvAPI.getServers();
                Response<MediaContainerServer> response = ajax.execute();


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
