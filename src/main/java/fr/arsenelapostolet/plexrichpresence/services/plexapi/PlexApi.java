package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import fr.arsenelapostolet.plexrichpresence.model.Server;
import fr.arsenelapostolet.plexrichpresence.model.User;

import java.util.function.Consumer;

public interface PlexApi {
    void setCredentials(String login, String password);

    void getServer(Consumer<Server> callback, Consumer<Throwable> failCallback);

    void getToken(Consumer<User> callback);

    void getSessions(Consumer<Metadatum> callback);
}
