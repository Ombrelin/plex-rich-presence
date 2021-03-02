package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import fr.arsenelapostolet.plexrichpresence.SharedVariables;
import fr.arsenelapostolet.plexrichpresence.model.*;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv.PlexApiHttpClient;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv.PlexTokenHttpClient;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv.PlexTvAPI;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.server.PlexSessionHttpClient;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import rx.Observable;

import java.net.InetSocketAddress;
import java.net.Socket;
import java.util.Arrays;
import java.util.List;
import java.util.concurrent.TimeUnit;
import java.util.stream.Collectors;

@Service
public class PlexApiImpl implements PlexApi {

    private final PlexTvAPI api = new PlexTokenHttpClient().getAPI();

    private final Logger LOG = LoggerFactory.getLogger(PlexApiImpl.class);

    public PlexApiImpl() {
    }

    @Override
    public Observable<List<Server>> getServers(String authToken) {
        return new PlexApiHttpClient()
                .getAPI()
                .getServers(authToken)
                .map(MediaContainerServer::getServer)
                .map(servers -> servers.stream().filter(this::isValid)
                        .collect(Collectors.toList()));

    }

    private boolean isValid(Server server) {
        return this.isWorking(server) && this.isOwned(server);
    }

    private boolean isWorking(Server server) {
        String[] result = checkServer(server);
        if (result[0].equals("success")) {
            server.setFinalAddress(result[1]);
            return true;
        } else {
            return false;
        }
    }

    private boolean isOwned(Server server) {
        return server.getOwned().equals("1");
    }

    @Override
    public Observable<User> getUser(String authToken) {
        return api.getUser(authToken, SharedVariables.plexClientIdentifier, SharedVariables.plexProduct);
    }

    @Override
    public Observable<List<Metadatum>> getSessions(List<Server> servers, String username) {

        List<Observable<PlexSessions>> sessionsObs =
                servers
                        .stream()
                        .map(server ->
                                new PlexSessionHttpClient(server.getFinalAddress(), server.getPort())
                                        .getAPI()
                                        .getSessions(server.getAccessToken(), "application/json"))
                        .collect(Collectors.toList());

        return Observable.zip(sessionsObs, sessions -> Arrays.stream(sessions)
                .filter(obj -> obj instanceof PlexSessions)
                .map(obj -> (PlexSessions) obj)
                .flatMap(session -> session.getMediaContainer().getMetadata().stream())
                .filter(session -> session.getUser().getTitle().equals(username))
                .collect(Collectors.toList()));
    }

    @Override
    public Observable<PlexAuth> getPlexAuthPin(boolean strong, String plexProduct, String plexClientId) {
        return api.generatePlexAuthPin(true, plexProduct, plexClientId);
    }

    @Override
    public Observable<PlexAuth> validatePlexAuthPin(int id, String code, String plexProduct) {
        Observable<PlexAuth> response;
        while (true) {
            response = api.validatePlexAuthPin(id, plexProduct, code);
            if (response.toBlocking().first().authToken != null) {
                return response;
            }
            try {
                TimeUnit.SECONDS.sleep(1);
            } catch (InterruptedException ignored) {
            }

        }
    }

    private String[] checkServer(Server server) {
        String[] result = new String[2];
        String localAddr = server.getLocalAddresses();
        String[] LocalServerAddresses = localAddr.split("\\s*,\\s*");

        LOG.debug("Trying server " + server.getAddress());
        if (serverListening(server.getAddress(), Integer.parseInt(server.getPort()))) {
            result[0] = "success";
            result[1] = server.getAddress();
            LOG.debug("Connected to server " + server.getAddress());
        } else {
            for (String address : LocalServerAddresses) {
                LOG.debug("Trying server " + address);
                if (serverListening(address, Integer.parseInt(server.getPort()))) {
                    result[0] = "success";
                    result[1] = address;
                    LOG.debug("Connected to server " + address);
                    return result;
                }
            }
            result[0] = "fail";
            result[1] = null;
            LOG.debug("Failed to find any servers");
        }
        return result;
    }

    private Boolean serverListening(String host, int port) {
        Socket socket = new Socket();
        try {
            socket.connect(new InetSocketAddress(host, port), 1000);
            socket.close();
            return true;
        } catch (Exception e) {
            return false;
        }
    }

}
