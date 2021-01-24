package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import fr.arsenelapostolet.plexrichpresence.model.PlexLogin;
import fr.arsenelapostolet.plexrichpresence.model.Server;
import rx.Observable;

import java.util.List;

public interface PlexApi {
    Observable<List<Server>> getServers(String username, String password);

    Observable<PlexLogin> getToken(String username, String password);

    Observable<List<Metadatum>> getSessions(List<Server> servers, String username);
}
