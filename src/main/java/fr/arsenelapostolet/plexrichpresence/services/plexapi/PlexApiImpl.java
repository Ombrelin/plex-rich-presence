package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import fr.arsenelapostolet.plexrichpresence.model.Metadatum;
import fr.arsenelapostolet.plexrichpresence.model.Server;
import fr.arsenelapostolet.plexrichpresence.model.User;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv.*;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.server.PlexServerAPI;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.server.PlexSessionAjax;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.server.PlexSessionHttpClient;
import org.springframework.cglib.core.Local;
import org.springframework.stereotype.Service;

import java.net.InetSocketAddress;
import java.net.Socket;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.function.Consumer;

@Service
public class PlexApiImpl implements PlexApi {

    private User user;
    private Server server;
    private String login;
    private String password;
    private String finalAddress;

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

            String localAddr = this.server.getLocalAddresses();
            List<String> LocalServerAddresses = Arrays.asList(localAddr.split("\\s*,\\s*"));;

            if (serverListening(this.server.getAddress(), Integer.parseInt(this.server.getPort()))) {
                finalAddress = this.server.getAddress();
            } else {
                for (String address: LocalServerAddresses) {
                    if (serverListening(address, Integer.parseInt(server.getPort()))){
                        finalAddress = address;
                        break;
                    }
                }
            }
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
                finalAddress,
                this.server.getPort()
        );
        PlexServerAPI api = client.getAPI();
        PlexSessionAjax ajax = new PlexSessionAjax(api, this.user.getAuthToken());
        ajax.setOnSucceeded(state -> {
            Metadatum userMetaDatum;
            try {
                userMetaDatum = ajax.getValue().getMediaContainer().getMetadata().stream()
                        .filter(session -> session.getUser().getTitle().equals(user.getUsername()))
                        .findAny()
                        .orElseThrow(IllegalArgumentException::new);
            } catch (NullPointerException e){
                userMetaDatum = null;
            }

            callback.accept(userMetaDatum);
        });
        ajax.start();
    }

    public static boolean serverListening(String host, int port)
    {
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
