package fr.arsenelapostolet.plexrichpresence.model;

import lombok.Data;
import org.simpleframework.xml.Attribute;

import java.net.InetSocketAddress;
import java.net.Socket;

@Data
public class Server {

    @Attribute private String name;
    @Attribute private String address;
    @Attribute private String port;
    @Attribute private String version;
    @Attribute private String scheme;
    @Attribute private String host;
    @Attribute private String localAddresses;
    @Attribute private String machineIdentifier;
    @Attribute private String createdAt;
    @Attribute private String updatedAt;
    @Attribute private String owned;
    @Attribute private String synced;
    @Attribute(required = false) private String sourceTitle;
    @Attribute(required = false) private String accessToken;
    @Attribute(required = false) private String ownerId;
    @Attribute(required = false) private String home;
    private String finalAddress;
}
