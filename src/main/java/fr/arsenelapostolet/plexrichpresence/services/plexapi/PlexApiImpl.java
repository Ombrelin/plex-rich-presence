package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import fr.arsenelapostolet.plexrichpresence.model.Server;
import fr.arsenelapostolet.plexrichpresence.model.User;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv.*;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.server.PlexServerAPI;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.server.PlexSessionAjax;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.server.PlexSessionHttpClient;
import org.springframework.stereotype.Service;

import java.util.function.Consumer;

@Service
public class PlexApiImpl implements PlexApi {

    private User user;
    private Server server;
    private String login;
    private String password;

    @Override
    public void setCredentials(String login, String password) {
        this.login = login;
        this.password = password;
    }

    @Override
    public void getServer(Consumer<Server> callback, Consumer<Throwable> failCallback) {
        PlexApiHttpClient client = new PlexApiHttpClient(
                this.login,
                this.password
        );
        PlexTvAPI api = client.getAPI();
        PlexServerAjax ajax = new PlexServerAjax(api);
        ajax.setOnSucceeded(state -> {
            callback.accept(ajax.getValue().getServer());
            this.server = ajax.getValue().getServer();
        });
        ajax.setOnFailed(state -> failCallback.accept(ajax.getException()));
        ajax.start();
    }

    @Override
    public void getToken(Consumer<User> callback) {
        PlexTokenHttpClient client = new PlexTokenHttpClient(
                this.login,
                this.password
        );
        PlexTvAPI api = client.getAPI();
        PlexTokenAjax ajax = new PlexTokenAjax(api, this.login, this.password);
        ajax.setOnSucceeded(state -> {
            this.user = ajax.getValue().getUser();
            callback.accept(ajax.getValue().getUser());
        });
        ajax.start();
    }

    @Override
    public void getSessions(Consumer<Metadatum> callback) {
        PlexSessionHttpClient client = new PlexSessionHttpClient(
                this.server.getAddress(),
                this.server.getPort()
        );
        PlexServerAPI api = client.getAPI();
        PlexSessionAjax ajax = new PlexSessionAjax(api, this.user.getAuthToken());
        ajax.setOnSucceeded(state -> {
            Metadatum userMetaDatum = ajax.getValue().getMediaContainer().getMetadata().stream()
                    .filter(session -> session.getUser().getTitle().equals(user.getUsername()))
                    .findAny()
                    .orElseThrow(IllegalArgumentException::new);
            callback.accept(userMetaDatum);
        });
        ajax.start();
    }
}
