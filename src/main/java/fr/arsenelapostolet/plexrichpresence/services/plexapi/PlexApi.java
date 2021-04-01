package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import fr.arsenelapostolet.plexrichpresence.model.PlexAuth;
import fr.arsenelapostolet.plexrichpresence.model.Server;
import fr.arsenelapostolet.plexrichpresence.model.User;
import rx.Observable;

import java.util.List;

public interface PlexApi {
    Observable<List<Server>> getServers(String authToken);

    Observable<User> getUser(String authToken);

    Observable<List<Metadatum>> getSessions(List<Server> servers, String username);

    Observable<PlexAuth> getPlexAuthPin(boolean strong, String plexProduct, String plexClientId);

    Observable<PlexAuth> validatePlexAuthPin(int id, String code, String plexProduct);
}
